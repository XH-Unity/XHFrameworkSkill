using System;
using System.Collections.Generic;
using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 投射物控制器
    /// 挂在投射物GameObject上，负责飞行逻辑和碰撞检测
    /// </summary>
    public class ProjectileController : MonoBehaviour
    {
        // ============ 事件 ============
        public event Action<AbilitySystemComponent, Vector2> OnHit;
        public event Action<Vector2> OnReachTarget;
        public event Action OnDestroy;

        // ============ 配置数据 ============
        private ProjectileInitData _data;
        private bool _initialized = false;

        // ============ 运行时状态 ============
        private Vector2 _currentPosition;
        private Vector2 _currentDirection;
        private float _traveledDistance;
        private float _totalDistance;
        private int _hitCount;
        private HashSet<AbilitySystemComponent> _hitTargets = new HashSet<AbilitySystemComponent>();
        private bool _reachedTarget = false;

        // ============ 曲线飞行 ============
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private float _flightProgress; // 0-1

        /// <summary>
        /// 初始化投射物
        /// </summary>
        public void Initialize(ProjectileInitData data)
        {
            _data = data;
            _initialized = true;

            _currentPosition = data.LaunchPosition;
            _currentDirection = data.Direction;
            _startPosition = data.LaunchPosition;
            _endPosition = data.TargetPosition;
            _totalDistance = Vector2.Distance(_startPosition, _endPosition);
            _traveledDistance = 0f;
            _flightProgress = 0f;
            _hitCount = 0;
            _hitTargets.Clear();
            _reachedTarget = false;

            // 设置初始朝向
            UpdateRotation();
        }

        private void Update()
        {
            if (!_initialized) return;

            float deltaTime = Time.deltaTime;

            // 更新位置
            UpdatePosition(deltaTime);

            // 碰撞检测
            CheckCollision();

            // 检查是否到达目标或超出距离
            CheckReachTarget();

            // 更新Transform
            transform.position = _currentPosition;
            UpdateRotation();
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        private void UpdatePosition(float deltaTime)
        {
            float moveDistance = _data.Speed * deltaTime;
            _traveledDistance += moveDistance;

            if (_data.TargetType == ProjectileTargetType.Unit)
            {
                // 单位模式：追踪目标
                UpdatePositionForUnit(moveDistance);
            }
            else
            {
                // 点模式：飞向目标点
                UpdatePositionForPosition(moveDistance);
            }
        }

        /// <summary>
        /// 单位模式：追踪飞行
        /// </summary>
        private void UpdatePositionForUnit(float moveDistance)
        {
            // 更新目标位置（如果目标还存在）
            if (_data.TargetUnit?.Owner != null)
            {
                _endPosition = GetTargetUnitPosition();
                _totalDistance = Vector2.Distance(_currentPosition, _endPosition);
            }

            if (_data.CurveHeight > 0 && _totalDistance > 0.1f)
            {
                // 曲线追踪：使用抛物线插值
                _flightProgress = Mathf.Clamp01(_traveledDistance / (_totalDistance + _traveledDistance));

                // 计算直线位置
                Vector2 linearPos = Vector2.MoveTowards(_currentPosition, _endPosition, moveDistance);

                // 添加曲线偏移
                float curveOffset = _data.CurveHeight * 4f * _flightProgress * (1f - _flightProgress);
                Vector2 perpendicular = GetPerpendicular(_currentDirection);

                _currentPosition = linearPos + perpendicular * curveOffset;
                _currentDirection = (_endPosition - _currentPosition).normalized;
            }
            else
            {
                // 直线追踪
                _currentDirection = (_endPosition - _currentPosition).normalized;
                _currentPosition = Vector2.MoveTowards(_currentPosition, _endPosition, moveDistance);
            }
        }

        /// <summary>
        /// 点模式：直线/曲线飞行
        /// </summary>
        private void UpdatePositionForPosition(float moveDistance)
        {
            if (_data.FlyOver)
            {
                // 飞跃模式：沿初始方向一直飞
                _currentPosition += _currentDirection * moveDistance;
            }
            else if (_data.CurveHeight > 0 && _totalDistance > 0.1f)
            {
                // 曲线飞行（抛物线）
                _flightProgress = Mathf.Clamp01(_traveledDistance / _totalDistance);

                // 计算直线位置
                Vector2 linearPos = Vector2.Lerp(_startPosition, _endPosition, _flightProgress);

                // 添加抛物线偏移（垂直于飞行方向）
                float curveOffset = _data.CurveHeight * 4f * _flightProgress * (1f - _flightProgress);
                Vector2 perpendicular = GetPerpendicular((_endPosition - _startPosition).normalized);

                _currentPosition = linearPos + perpendicular * curveOffset;

                // 更新方向（用于旋转）
                if (_flightProgress < 1f)
                {
                    float nextProgress = Mathf.Clamp01((_traveledDistance + 0.1f) / _totalDistance);
                    Vector2 nextLinearPos = Vector2.Lerp(_startPosition, _endPosition, nextProgress);
                    float nextCurveOffset = _data.CurveHeight * 4f * nextProgress * (1f - nextProgress);
                    Vector2 nextPos = nextLinearPos + perpendicular * nextCurveOffset;
                    _currentDirection = (nextPos - _currentPosition).normalized;
                }
            }
            else
            {
                // 直线飞行
                _currentPosition += _currentDirection * moveDistance;
            }
        }

        /// <summary>
        /// 获取目标单位位置（考虑挂点）
        /// </summary>
        private Vector2 GetTargetUnitPosition()
        {
            if (_data.TargetUnit?.Owner == null)
                return _endPosition;

            var transform = _data.TargetUnit.Owner.transform;

            if (!string.IsNullOrEmpty(_data.TargetBindingName))
            {
                var bindingPoint = transform.Find(_data.TargetBindingName);
                if (bindingPoint != null)
                    return bindingPoint.position;
            }

            return transform.position;
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        private void CheckCollision()
        {
            // 使用Physics2D.OverlapCircleAll检测碰撞
            var colliders = Physics2D.OverlapCircleAll(_currentPosition, _data.CollisionRadius);

            foreach (var collider in colliders)
            {
                var asc = GetASCFromCollider(collider);
                if (asc == null) continue;

                // 跳过已命中的目标
                if (_hitTargets.Contains(asc)) continue;

                // 跳过发射者
                if (asc == _data.SourceASC) continue;

                // 检查标签
                if (!IsValidTarget(asc)) continue;

                // 命中！
                _hitTargets.Add(asc);
                _hitCount++;

                // 触发命中事件
                OnHit?.Invoke(asc, _currentPosition);

                // 检查是否需要销毁
                if (!_data.IsPiercing)
                {
                    // 不穿透，直接销毁
                    DestroyProjectile();
                    return;
                }
                else if (_hitCount >= _data.MaxPierceCount)
                {
                    // 达到最大穿透数，销毁
                    DestroyProjectile();
                    return;
                }
            }
        }

        /// <summary>
        /// 检查是否到达目标
        /// </summary>
        private void CheckReachTarget()
        {
            if (_data.TargetType == ProjectileTargetType.Unit)
            {
                // 单位模式：检查是否碰到目标单位（由碰撞检测处理）
                // 如果目标死亡，继续飞行直到超出范围
                if (_data.TargetUnit?.Owner == null)
                {
                    // 目标已死亡，检查最大距离
                    if (_data.MaxDistance > 0 && _traveledDistance >= _data.MaxDistance)
                    {
                        DestroyProjectile();
                    }
                }
            }
            else
            {
                // 点模式
                if (_data.FlyOver)
                {
                    // 飞跃模式：检查是否经过目标点
                    if (!_reachedTarget)
                    {
                        float distToTarget = Vector2.Distance(_currentPosition, _endPosition);
                        if (distToTarget < _data.CollisionRadius || _traveledDistance >= _totalDistance)
                        {
                            _reachedTarget = true;
                            OnReachTarget?.Invoke(_endPosition);
                        }
                    }

                    // 检查最大距离
                    if (_data.MaxDistance > 0 && _traveledDistance >= _data.MaxDistance)
                    {
                        DestroyProjectile();
                    }
                }
                else
                {
                    // 非飞跃模式：到达目标点就停止
                    if (_flightProgress >= 1f || Vector2.Distance(_currentPosition, _endPosition) < 0.1f)
                    {
                        OnReachTarget?.Invoke(_endPosition);
                        DestroyProjectile();
                    }

                    // 检查最大距离
                    if (_data.MaxDistance > 0 && _traveledDistance >= _data.MaxDistance)
                    {
                        DestroyProjectile();
                    }
                }
            }
        }

        /// <summary>
        /// 从Collider2D获取AbilitySystemComponent
        /// </summary>
        private AbilitySystemComponent GetASCFromCollider(Collider2D collider)
        {
            if (collider == null) return null;

            var unit = collider.GetComponent<Unit>();
            if (unit != null) return unit.ownerASC;

            unit = collider.GetComponentInParent<Unit>();
            if (unit != null) return unit.ownerASC;

            return null;
        }

        /// <summary>
        /// 检查目标是否有效
        /// </summary>
        private bool IsValidTarget(AbilitySystemComponent target)
        {
            if (target == null) return false;

            // 检查目标标签（必须拥有）
            if (!_data.CollisionTargetTags.IsEmpty && !target.HasAnyTags(_data.CollisionTargetTags))
                return false;

            // 检查排除标签
            if (!_data.CollisionExcludeTags.IsEmpty && target.HasAnyTags(_data.CollisionExcludeTags))
                return false;

            return true;
        }

        /// <summary>
        /// 获取垂直向量（用于曲线计算）
        /// 正数向上弯曲，负数向下弯曲
        /// </summary>
        private Vector2 GetPerpendicular(Vector2 dir)
        {
            // 返回向上的垂直向量（左手法则：dir顺时针旋转90°）
            return new Vector2(dir.y, -dir.x);
        }

        /// <summary>
        /// 更新旋转（朝向飞行方向）
        /// </summary>
        private void UpdateRotation()
        {
            if (_currentDirection.sqrMagnitude > 0.001f)
            {
                float angle = Mathf.Atan2(_currentDirection.y, _currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// 销毁投射物
        /// </summary>
        private void DestroyProjectile()
        {
            _initialized = false;
            OnDestroy?.Invoke();

            // 清理事件
            OnHit = null;
            OnReachTarget = null;
            OnDestroy = null;

            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            if (!_initialized) return;

            // 绘制碰撞半径
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_currentPosition, _data.CollisionRadius);

            // 绘制目标位置
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_endPosition, 0.2f);

            // 绘制飞行路径
            Gizmos.color = Color.green;
            Gizmos.DrawLine(_currentPosition, _endPosition);
        }
    }
}
