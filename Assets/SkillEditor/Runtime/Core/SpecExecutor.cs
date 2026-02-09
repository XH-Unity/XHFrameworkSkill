using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Spec执行器 - 独立于AbilitySpec的节点执行工具
    /// 重构后支持 Effect、Task、Condition、Cue 四种节点类型
    ///
    /// 生命周期管理规则：
    /// - Cue 由触发它的 Effect 管理，Effect结束时Cancel
    /// - 瞬时Effect 执行完即结束，不需要管理
    /// - 持续/周期Effect 由EffectContainer管理，或通过标签自动管理
    /// - Task 和 Condition 都是瞬时执行，不需要管理
    /// </summary>
    public static class SpecExecutor
    {
        /// <summary>
        /// 执行指定端口连接的所有节点
        /// </summary>
        /// <param name="skillId">技能ID</param>
        /// <param name="nodeGuid">当前节点Guid</param>
        /// <param name="outputPortName">输出端口名称</param>
        /// <param name="context">执行上下文</param>
        public static void ExecuteConnectedNodes(string skillId, string nodeGuid, string outputPortName, SpecExecutionContext context)
        {
            if (string.IsNullOrEmpty(skillId) || string.IsNullOrEmpty(nodeGuid))
                return;

            var connectedNodes = SkillDataCenter.Instance.GetConnectedNodes(skillId, nodeGuid, outputPortName);
            if (connectedNodes == null || connectedNodes.Count == 0)
                return;

            foreach (var nodeData in connectedNodes)
            {
                ExecuteNode(skillId, nodeData, context);
            }
        }

        /// <summary>
        /// 执行指定端口连接的Cue节点，并返回触发的CueSpec列表
        /// 用于需要管理Cue生命周期的场景（如动画时间Cue）
        /// </summary>
        public static List<GameplayCueSpec> ExecuteConnectedCueNodes(string skillId, string nodeGuid, string outputPortName, SpecExecutionContext context)
        {
            var triggeredCues = new List<GameplayCueSpec>();

            if (string.IsNullOrEmpty(skillId) || string.IsNullOrEmpty(nodeGuid))
                return triggeredCues;

            var connectedNodes = SkillDataCenter.Instance.GetConnectedNodes(skillId, nodeGuid, outputPortName);
            if (connectedNodes == null || connectedNodes.Count == 0)
                return triggeredCues;

            foreach (var nodeData in connectedNodes)
            {
                var category = GetNodeCategory(nodeData.nodeType);
                if (category == NodeCategory.Cue)
                {
                    var cueSpec = ExecuteCueNodeAndReturn(skillId, nodeData, context);
                    if (cueSpec != null)
                    {
                        triggeredCues.Add(cueSpec);
                    }
                }
                else
                {
                    // 非Cue节点正常执行
                    ExecuteNode(skillId, nodeData, context);
                }
            }

            return triggeredCues;
        }

        /// <summary>
        /// 执行单个节点
        /// </summary>
        public static void ExecuteNode(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            if (nodeData == null)
                return;

            var category = GetNodeCategory(nodeData.nodeType);

            switch (category)
            {
                case NodeCategory.Effect:
                    ExecuteEffectNode(skillId, nodeData, context);
                    break;

                case NodeCategory.Task:
                    ExecuteTaskNode(skillId, nodeData, context);
                    break;

                case NodeCategory.Condition:
                    ExecuteConditionNode(skillId, nodeData, context);
                    break;

                case NodeCategory.Cue:
                    ExecuteCueNode(skillId, nodeData, context);
                    break;
            }
        }

        /// <summary>
        /// 执行效果节点
        /// </summary>
        private static void ExecuteEffectNode(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            var effectSpec = SpecFactory.CreateEffectSpec(nodeData.nodeType);
            if (effectSpec == null)
                return;

            effectSpec.Initialize(skillId, nodeData.guid, context);
            effectSpec.Execute();

            // 如果是持续/周期效果且正在运行，注册到对应的Owner
            if (effectSpec.IsRunning && effectSpec.EffectNodeData?.durationType != EffectDurationType.Instant)
            {
                RegisterRunningEffect(effectSpec, context);
            }
        }

        /// <summary>
        /// 执行任务节点
        /// </summary>
        private static void ExecuteTaskNode(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            var taskSpec = SpecFactory.CreateTaskSpec(nodeData.nodeType);
            if (taskSpec == null)
                return;

            taskSpec.Initialize(skillId, nodeData.guid, context);
            taskSpec.Execute();
        }

        /// <summary>
        /// 执行条件节点
        /// </summary>
        private static void ExecuteConditionNode(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            var conditionSpec = SpecFactory.CreateConditionSpec(nodeData.nodeType);
            if (conditionSpec == null)
                return;

            conditionSpec.Initialize(skillId, nodeData.guid, context);
            conditionSpec.Execute();
        }

        /// <summary>
        /// 注册运行中的Effect到对应的Owner
        /// </summary>
        private static void RegisterRunningEffect(GameplayEffectSpec effectSpec, SpecExecutionContext context)
        {
            if (context == null)
                return;
            // 否则注册到技能
             if (context.AbilitySpec != null && context.AbilitySpec.IsRunning&&effectSpec.EffectNodeData.cancelOnAbilityEnd)
            {
                context.AbilitySpec.RegisterRunningEffect(effectSpec);
            }
        }

        /// <summary>
        /// 执行Cue节点
        /// </summary>
        private static void ExecuteCueNode(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            var cueSpec = SpecFactory.CreateCueSpec(nodeData.nodeType);
            if (cueSpec == null)
                return;

            cueSpec.Initialize(skillId, nodeData.guid, context);
            cueSpec.Execute();

            // 如果Cue正在运行，注册到对应的Owner
            if (cueSpec.IsRunning)
            {
                RegisterRunningCue(cueSpec, context);
            }
        }

        /// <summary>
        /// 执行Cue节点并返回CueSpec（用于外部管理生命周期）
        /// </summary>
        private static GameplayCueSpec ExecuteCueNodeAndReturn(string skillId, NodeData nodeData, SpecExecutionContext context)
        {
            var cueSpec = SpecFactory.CreateCueSpec(nodeData.nodeType);
            if (cueSpec == null)
                return null;

            cueSpec.Initialize(skillId, nodeData.guid, context);
            cueSpec.Execute();

            // 注意：这里不注册到Owner，由调用方管理生命周期
            return cueSpec;
        }

        /// <summary>
        /// 注册运行中的Cue到对应的Owner
        /// </summary>
        private static void RegisterRunningCue(GameplayCueSpec cueSpec, SpecExecutionContext context)
        {
            if (context == null)
                return;

            // 如果是Effect触发的Cue，注册到Effect
            if (context.OwnerEffectSpec != null && cueSpec.DestroyWithNode)
            {
                context.OwnerEffectSpec.RegisterTriggeredCue(cueSpec);
            }
            // 没有Owner的Cue会自己管理生命周期（自然结束）
        }

        /// <summary>
        /// 获取节点分类
        /// </summary>
        private static NodeCategory GetNodeCategory(NodeType nodeType)
        {
            switch (nodeType)
            {
                // Task节点
                case NodeType.SearchTargetTask:
                case NodeType.EndAbilityTask:
                    return NodeCategory.Task;

                // Condition节点
                case NodeType.AttributeCompareCondition:
                    return NodeCategory.Condition;

                // Cue节点
                case NodeType.ParticleCue:
                case NodeType.SoundCue:
                case NodeType.FloatingTextCue:
                    return NodeCategory.Cue;

                // 其他所有节点都作为Effect处理
                default:
                    return NodeCategory.Effect;
            }
        }
    }
}
