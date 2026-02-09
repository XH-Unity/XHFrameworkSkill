using System;
using System.Collections.Generic;
using UnityEngine;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 节点工厂 - 负责创建各类型节点
    /// </summary>
    public static class NodeFactory
    {
        private static readonly Dictionary<NodeType, Func<Vector2, SkillNodeBase>> nodeCreators = new()
        {
            // ============ 技能节点 ============
            { NodeType.Ability, pos => new AbilityNode(pos) },

            // ============ 效果节点 (Effect) ============

            // --- 瞬时效果 (Instant) ---
            { NodeType.DamageEffect, pos => new DamageEffectNode(pos) },
            { NodeType.HealEffect, pos => new HealEffectNode(pos) },
            { NodeType.CostEffect, pos => new CostEffectNode(pos) },
            { NodeType.ProjectileEffect, pos => new ProjectileEffectNode(pos) },
            { NodeType.PlacementEffect, pos => new PlacementEffectNode(pos) },

            // --- 持续效果 (Duration) ---
            { NodeType.CooldownEffect, pos => new CooldownEffectNode(pos) },
            { NodeType.BuffEffect, pos => new BuffEffectNode(pos) },
            { NodeType.ModifyAttributeEffect, pos => new ModifyAttributeEffectNode(pos) },
            { NodeType.GenericEffect, pos => new GenericEffectNode(pos) },
            { NodeType.DisplaceEffect, pos => new DisplaceEffectNode(pos) },

            // ============ 任务节点 (Task) ============
            { NodeType.SearchTargetTask, pos => new SearchTargetTaskNode(pos) },
            { NodeType.EndAbilityTask, pos => new EndAbilityTaskNode(pos) },

            // ============ 条件节点 (Condition) ============
            { NodeType.AttributeCompareCondition, pos => new AttributeCompareConditionNode(pos) },

            // ============ 表现节点 (Cue) ============
            { NodeType.ParticleCue, pos => new ParticleCueNode(pos) },
            { NodeType.SoundCue, pos => new SoundCueNode(pos) },
            { NodeType.FloatingTextCue, pos => new FloatingTextCueNode(pos) },

            // ============ 动画节点 ============
            { NodeType.Animation, pos => new AnimationNode(pos) },
        };
        

        /// <summary>
        /// 根据节点类型创建节点
        /// </summary>
        public static SkillNodeBase CreateNode(NodeType nodeType, Vector2 position)
        {
            if (nodeCreators.TryGetValue(nodeType, out var creator))
            {
                return creator(position);
            }

            Debug.LogError($"未知的节点类型: {nodeType}");
            return null;
        }

        /// <summary>
        /// 从NodeData创建节点（用于加载）
        /// </summary>
        public static SkillNodeBase CreateNodeFromData(NodeData data)
        {
            var node = CreateNode(data.nodeType, data.position);
            if (node != null)
            {
                node.Guid = data.guid;
                node.LoadData(data);
            }
            return node;
        }

        /// <summary>
        /// 注册自定义节点类型
        /// </summary>
        public static void RegisterNodeType(NodeType nodeType, Func<Vector2, SkillNodeBase> creator)
        {
            nodeCreators[nodeType] = creator;
        }

        /// <summary>
        /// 获取节点分类
        /// </summary>
        public static NodeCategory GetNodeCategory(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.Ability:
                    return NodeCategory.Root;

                case NodeType.DamageEffect:
                case NodeType.HealEffect:
                case NodeType.CostEffect:
                case NodeType.ProjectileEffect:
                case NodeType.PlacementEffect:
                case NodeType.CooldownEffect:
                case NodeType.BuffEffect:
                case NodeType.ModifyAttributeEffect:
                case NodeType.GenericEffect:
                case NodeType.DisplaceEffect:
                    return NodeCategory.Effect;

                case NodeType.SearchTargetTask:
                case NodeType.EndAbilityTask:
                    return NodeCategory.Task;

                case NodeType.AttributeCompareCondition:
                    return NodeCategory.Condition;

                case NodeType.ParticleCue:
                case NodeType.SoundCue:
                case NodeType.FloatingTextCue:
                    return NodeCategory.Cue;

                case NodeType.Animation:
                    return NodeCategory.Task;

                default:
                    return NodeCategory.Root;
            }
        }
    }
}
