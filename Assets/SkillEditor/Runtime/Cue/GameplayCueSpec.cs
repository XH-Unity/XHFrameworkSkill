using System;
using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Cue Spec基类 - 包含动态数据和执行逻辑
    /// Cue节点用于播放视觉/音效表现，不改变游戏状态
    /// </summary>
    public abstract class GameplayCueSpec
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string SpecId { get; private set; }

        /// <summary>
        /// 技能ID（用于从数据中心获取数据）
        /// </summary>
        public string SkillId { get; private set; }

        /// <summary>
        /// 节点Guid
        /// </summary>
        public string NodeGuid { get; private set; }

        /// <summary>
        /// 执行上下文
        /// </summary>
        public SpecExecutionContext Context { get; private set; }

        /// <summary>
        /// 是否正在执行
        /// </summary>
        public bool IsRunning { get; protected set; }

        /// <summary>
        /// 是否已取消
        /// </summary>
        protected bool IsCancelled { get; private set; }

        // ============ 动态数据 ============

        /// <summary>
        /// 标签容器
        /// </summary>
        public CueTagContainer Tags { get; protected set; }

        /// <summary>
        /// 激活的Cue实例
        /// </summary>
        protected ActiveGameplayCue ActiveCue { get; set; }

        // ============ 静态数据访问 ============

        /// <summary>
        /// 获取节点数据（从数据中心）
        /// </summary>
        protected NodeData NodeData => SkillDataCenter.Instance.GetNodeData(SkillId, NodeGuid);

        /// <summary>
        /// 获取Cue节点数据
        /// </summary>
        protected CueNodeData CueNodeData => NodeData as CueNodeData;

        // ============ 初始化 ============
        /// <summary>
        /// 随节点销毁
        /// </summary>
        public bool DestroyWithNode { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        public virtual void Initialize(string skillId, string nodeGuid, SpecExecutionContext context)
        {
            SpecId = Guid.NewGuid().ToString();
            SkillId = skillId;
            NodeGuid = nodeGuid;
            Context = context;
            IsRunning = false;
            IsCancelled = false;

            // 从静态数据复制标签配置
            var cueData = CueNodeData;
            if (cueData != null)
            {
                Tags = new CueTagContainer(cueData);
            }

            // 子类初始化
            OnInitialize();
        }

        /// <summary>
        /// 子类初始化回调
        /// </summary>
        protected virtual void OnInitialize() { }

        // ============ 执行方法 ============

        /// <summary>
        /// 执行Cue
        /// </summary>
        public virtual void Execute()
        {
            if (Context == null)
                return;

            // 获取目标
            var target = GetTarget();

            // 检查标签条件
            if (!CanPlayOnTarget(target))
                return;

            // 播放Cue
            PlayCue(target);
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public virtual void Tick(float deltaTime)
        {
            if (!IsRunning || ActiveCue == null)
                return;

            // 检查Cue是否已过期
            if (ActiveCue.IsExpired)
            {
                IsRunning = false;
                ActiveCue = null;
            }
        }

        /// <summary>
        /// 播放Cue（子类必须实现）
        /// </summary>
        protected abstract void PlayCue(AbilitySystemComponent target);

        /// <summary>
        /// 停止Cue（子类必须实现）
        /// </summary>
        protected abstract void StopCue();

        /// <summary>
        /// 取消执行
        /// </summary>
        public virtual void Cancel()
        {
            IsCancelled = true;
            IsRunning = false;
            StopCue();
        }

        /// <summary>
        /// 停止Cue（公开方法，供外部调用）
        /// </summary>
        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;
            StopCue();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            IsRunning = false;
            IsCancelled = false;
            ActiveCue = null;
        }

        // ============ 检查方法 ============

        /// <summary>
        /// 检查是否可以在目标上播放
        /// </summary>
        protected bool CanPlayOnTarget(AbilitySystemComponent target)
        {
            if (target == null)
                return true; // 无目标时允许播放（可能是世界空间Cue）

            // 检查所需标签
            if (!Tags.RequiredTags.IsEmpty)
            {
                if (!target.HasAllTags(Tags.RequiredTags))
                    return false;
            }

            // 检查免疫标签
            if (!Tags.ImmunityTags.IsEmpty)
            {
                if (target.HasAnyTags(Tags.ImmunityTags))
                    return false;
            }

            return true;
        }

        // ============ 辅助方法 ============

        /// <summary>
        /// 获取目标
        /// </summary>
        protected AbilitySystemComponent GetTarget()
        {
            var nodeData = NodeData;
            if (nodeData == null)
                return Context?.MainTarget;

            return Context?.GetTarget(nodeData.targetType);
        }

        /// <summary>
        /// 获取目标列表
        /// </summary>
        protected List<AbilitySystemComponent> GetTargets()
        {
            var nodeData = NodeData;
            if (nodeData == null)
                return Context?.Targets;

            return Context?.GetTargets(nodeData.targetType);
        }

        /// <summary>
        /// 获取目标的Transform
        /// </summary>
        protected UnityEngine.Transform GetTargetTransform(AbilitySystemComponent target)
        {
            return target?.Owner?.transform;
        }

        /// <summary>
        /// 获取目标的位置
        /// </summary>
        protected UnityEngine.Vector3 GetTargetPosition(AbilitySystemComponent target)
        {
            var transform = GetTargetTransform(target);
            return transform != null ? transform.position : UnityEngine.Vector3.zero;
        }
    }
}
