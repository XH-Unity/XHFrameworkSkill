using System;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 任务Spec基类 - 用于执行特定任务的节点
    /// 特点：瞬时执行、无属性修改、无堆叠、无持续时间
    /// </summary>
    public abstract class TaskSpec
    {
        // ============ 基础标识 ============
        public string SpecId { get; private set; }
        public string SkillId { get; private set; }
        public string NodeGuid { get; private set; }
        public SpecExecutionContext Context { get; private set; }
        public AbilitySystemComponent Source { get; private set; }

        // ============ 静态数据访问 ============
        protected NodeData NodeData => SkillDataCenter.Instance.GetNodeData(SkillId, NodeGuid);
        public TaskNodeData TaskNodeData => NodeData as TaskNodeData;

        // ============ 初始化 ============
        public virtual void Initialize(string skillId, string nodeGuid, SpecExecutionContext context)
        {
            SpecId = Guid.NewGuid().ToString();
            SkillId = skillId;
            NodeGuid = nodeGuid;
            Context = context;
            Source = context?.Caster;

            OnInitialize();
        }

        protected virtual void OnInitialize()
        {
        }

        // ============ 执行入口 ============
        public virtual void Execute()
        {
            if (Context == null) return;

            var target = GetTarget();
            OnExecute(target);
        }

        /// <summary>
        /// 子类实现具体的执行逻辑
        /// </summary>
        protected abstract void OnExecute(AbilitySystemComponent target);

        /// <summary>
        /// 获取执行上下文
        /// </summary>
        protected virtual SpecExecutionContext GetExecutionContext()
        {
            return Context;
        }

        // ============ 辅助方法 ============
        protected AbilitySystemComponent GetTarget()
        {
            var nodeData = NodeData;
            return nodeData == null ? Context?.MainTarget : Context?.GetTarget(nodeData.targetType);
        }
    }
}
