using SkillEditor.Data;
using UnityEngine;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 位移效果Spec - 持续移动目标位置（吸引/击退/吸引到指定点）
    /// 利用基类的 Duration/Tick 机制实现逐帧位移
    /// </summary>
    public class DisplaceEffectSpec : GameplayEffectSpec
    {
        private DisplaceEffectNodeData DisplaceNodeData => NodeData as DisplaceEffectNodeData;

        // 位移运行时状态
        private Vector3 _displaceDirection;
        private Vector3 _targetPoint;
        private Vector3 _startPosition;
        private float _movedDistance;
        private Transform _targetTransform;

        protected override void OnInitialHook(AbilitySystemComponent target)
        {
            if (target?.Owner == null) return;

            var nodeData = DisplaceNodeData;
            if (nodeData == null) return;

            _targetTransform = target.Owner.transform;
            _startPosition = _targetTransform.position;
            _movedDistance = 0f;

            Vector3 casterPos = Context?.Caster?.Owner != null
                ? Context.Caster.Owner.transform.position
                : _startPosition;

            switch (nodeData.displaceType)
            {
                case DisplaceType.Pull:
                    // 吸引：方向指向施法者
                    _targetPoint = casterPos;
                    _displaceDirection = (casterPos - _startPosition).normalized;
                    break;

                case DisplaceType.Push:
                    // 击退：方向远离施法者
                    _displaceDirection = (_startPosition - casterPos).normalized;
                    _targetPoint = _startPosition + _displaceDirection * nodeData.distance;
                    break;

                case DisplaceType.PullToPoint:
                    // 吸引到指定点
                    _targetPoint = Context.GetPosition(nodeData.pointSource, nodeData.pointBindingName);
                    _displaceDirection = (_targetPoint - _startPosition).normalized;
                    break;
            }

            // 方向为零（目标和施法者重叠）时不位移
            if (_displaceDirection.sqrMagnitude < 0.001f)
            {
                Expire();
            }
        }

        public override void Tick(float deltaTime)
        {
            // 先调用基类Tick处理超时
            base.Tick(deltaTime);

            if (IsExpired || !IsApplied || _targetTransform == null) return;

            var nodeData = DisplaceNodeData;
            if (nodeData == null) return;

            float moveStep = nodeData.speed * deltaTime;
            _movedDistance += moveStep;

            // 检查是否到达最大距离
            if (_movedDistance >= nodeData.distance)
            {
                Expire();
                return;
            }

            Vector3 currentPos = _targetTransform.position;

            switch (nodeData.displaceType)
            {
                case DisplaceType.Pull:
                {
                    // 吸引：检查是否到达最小距离
                    Vector3 casterPos = Context?.Caster?.Owner != null
                        ? Context.Caster.Owner.transform.position
                        : _targetPoint;
                    float distToCaster = Vector3.Distance(currentPos, casterPos);
                    if (distToCaster <= nodeData.minDistance)
                    {
                        Expire();
                        return;
                    }
                    // 实时更新方向（施法者可能在移动）
                    _displaceDirection = (casterPos - currentPos).normalized;
                    _targetTransform.position = currentPos + _displaceDirection * moveStep;
                    break;
                }

                case DisplaceType.Push:
                {
                    _targetTransform.position = currentPos + _displaceDirection * moveStep;
                    break;
                }

                case DisplaceType.PullToPoint:
                {
                    float distToPoint = Vector3.Distance(currentPos, _targetPoint);
                    if (distToPoint <= nodeData.minDistance)
                    {
                        Expire();
                        return;
                    }
                    _displaceDirection = (_targetPoint - currentPos).normalized;
                    _targetTransform.position = currentPos + _displaceDirection * moveStep;
                    break;
                }
            }
        }
    }
}
