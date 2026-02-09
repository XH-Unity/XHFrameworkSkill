using System;
using System.Collections.Generic;
using System.Linq;
using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 放置物控制器
    /// 挂在放置物GameObject上，负责碰撞检测
    /// 支持进入/离开/停留三种事件
    /// </summary>
    public class PlacementController : MonoBehaviour
    {
        // ============ 事件 ============

        /// <summary>
        /// 目标进入范围时触发（每个目标只触发一次）
        /// </summary>
        public event Action<AbilitySystemComponent> OnEnter;

        /// <summary>
        /// 目标离开范围时触发
        /// </summary>
        public event Action<AbilitySystemComponent> OnExit;

        // ============ 配置数据 ============
        private PlacementInitData _data;
        private bool _initialized = false;

        // ============ 运行时状态 ============

        /// <summary>
        /// 当前在范围内的目标
        /// </summary>
        private HashSet<AbilitySystemComponent> _currentTargets = new HashSet<AbilitySystemComponent>();

        /// <summary>
        /// 曾经进入过的目标（用于"只触发一次"的场景）
        /// </summary>
        private HashSet<AbilitySystemComponent> _everEnteredTargets = new HashSet<AbilitySystemComponent>();

        /// <summary>
        /// 初始化放置物
        /// </summary>
        public void Initialize(PlacementInitData data)
        {
            _data = data;
            _initialized = true;
            _currentTargets.Clear();
            _everEnteredTargets.Clear();
        }

        private void Update()
        {
            if (!_initialized) return;

            // 碰撞检测
            CheckCollision();
        }

        /// <summary>
        /// 碰撞检测 - 检测进入和离开
        /// </summary>
        private void CheckCollision()
        {
            // 获取当前范围内的所有目标
            var collidersInRange = Physics2D.OverlapCircleAll(transform.position, _data.CollisionRadius);
            var targetsInRange = new HashSet<AbilitySystemComponent>();

            foreach (var collider in collidersInRange)
            {
                var asc = GetASCFromCollider(collider);
                if (asc == null) continue;

                // 跳过施法者
                if (asc == _data.SourceASC) continue;

                // 检查标签
                if (!IsValidTarget(asc)) continue;

                targetsInRange.Add(asc);
            }

            // 检测新进入的目标
            foreach (var target in targetsInRange)
            {
                if (!_currentTargets.Contains(target))
                {
                    // 新目标进入
                    _currentTargets.Add(target);
                    _everEnteredTargets.Add(target);

                    // 触发进入事件
                    OnEnter?.Invoke(target);
                }
            }

            // 检测离开的目标
            var leftTargets = _currentTargets.Where(t => !targetsInRange.Contains(t)).ToList();
            foreach (var target in leftTargets)
            {
                _currentTargets.Remove(target);

                // 触发离开事件
                OnExit?.Invoke(target);
            }
        }

        /// <summary>
        /// 获取当前在范围内的所有目标
        /// </summary>
        public IReadOnlyCollection<AbilitySystemComponent> GetCurrentTargets()
        {
            return _currentTargets;
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
        /// 清除所有目标记录
        /// </summary>
        public void ClearAllTargets()
        {
            _currentTargets.Clear();
            _everEnteredTargets.Clear();
        }

        /// <summary>
        /// 放置物销毁时，触发所有当前目标的离开事件
        /// </summary>
        public void TriggerAllExitEvents()
        {
            foreach (var target in _currentTargets.ToList())
            {
                OnExit?.Invoke(target);
            }
            _currentTargets.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            if (!_initialized) return;

            // 绘制碰撞范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _data.CollisionRadius);
        }
    }
}
