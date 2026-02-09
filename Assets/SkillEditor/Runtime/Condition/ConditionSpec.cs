using System;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 条件Spec基类 - 用于条件判断分支的节点
    /// 特点：瞬时执行、返回布尔结果、根据结果执行不同分支
    /// </summary>
    public abstract class ConditionSpec
    {
        // ============ 基础标识 ============
        public string SpecId { get; private set; }
        public string SkillId { get; private set; }
        public string NodeGuid { get; private set; }
        public SpecExecutionContext Context { get; private set; }
        public AbilitySystemComponent Source { get; private set; }

        // ============ 静态数据访问 ============
        protected NodeData NodeData => SkillDataCenter.Instance.GetNodeData(SkillId, NodeGuid);
        public ConditionNodeData ConditionNodeData => NodeData as ConditionNodeData;

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
            bool result = Evaluate(target);

            // 根据结果执行对应分支
            SpecExecutor.ExecuteConnectedNodes(SkillId, NodeGuid, result ? "是" : "否", GetExecutionContext());
        }

        /// <summary>
        /// 子类实现具体的条件判断逻辑
        /// </summary>
        protected abstract bool Evaluate(AbilitySystemComponent target);

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
