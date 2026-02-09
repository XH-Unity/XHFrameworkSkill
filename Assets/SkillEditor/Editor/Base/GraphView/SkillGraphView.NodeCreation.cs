using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public partial class SkillGraphView
    {
        private IManipulator CreateContextualMenu()
        {
            var menuManipulator = new ContextualMenuManipulator(evt =>
            {
                var graphMousePosition = contentViewContainer.WorldToLocal(evt.mousePosition);
                pastePosition = graphMousePosition;

                // ============ 技能节点 ============
                evt.menu.AppendAction("创建节点/技能", _ => CreateNode(NodeType.Ability, graphMousePosition));

                evt.menu.AppendSeparator("创建节点/");

                // ============ 效果节点 (Effect) ============

                
                // --- 通用效果 ---
                evt.menu.AppendAction("创建节点/效果节点/通用效果", _ => CreateNode(NodeType.GenericEffect, graphMousePosition));
                
                // --- 瞬时效果 ---
                evt.menu.AppendAction("创建节点/效果节点/瞬时效果/伤害", _ => CreateNode(NodeType.DamageEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/瞬时效果/治疗", _ => CreateNode(NodeType.HealEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/瞬时效果/消耗", _ => CreateNode(NodeType.CostEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/瞬时效果/属性修改", _ => CreateNode(NodeType.ModifyAttributeEffect, graphMousePosition));

              

                // --- 持续效果 ---
                evt.menu.AppendAction("创建节点/效果节点/持续效果/冷却", _ => CreateNode(NodeType.CooldownEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/持续效果/Buff", _ => CreateNode(NodeType.BuffEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/持续效果/放置物", _ => CreateNode(NodeType.PlacementEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/持续效果/位移", _ => CreateNode(NodeType.DisplaceEffect, graphMousePosition));
                evt.menu.AppendAction("创建节点/效果节点/永久效果/投射物", _ => CreateNode(NodeType.ProjectileEffect, graphMousePosition));

                evt.menu.AppendSeparator("创建节点/");

                // ============ 任务节点 (Task) ============
                evt.menu.AppendAction("创建节点/任务节点/搜索目标", _ => CreateNode(NodeType.SearchTargetTask, graphMousePosition));
                evt.menu.AppendAction("创建节点/任务节点/结束技能", _ => CreateNode(NodeType.EndAbilityTask, graphMousePosition));
                evt.menu.AppendAction("创建节点/任务节点/动画", _ => CreateNode(NodeType.Animation, graphMousePosition));

                evt.menu.AppendSeparator("创建节点/");

                // ============ 条件节点 (Condition) ============
                evt.menu.AppendAction("创建节点/条件节点/属性比较", _ => CreateNode(NodeType.AttributeCompareCondition, graphMousePosition));

                evt.menu.AppendSeparator("创建节点/");

                // ============ 表现节点 (Cue) ============
                evt.menu.AppendAction("创建节点/表现节点/特效", _ => CreateNode(NodeType.ParticleCue, graphMousePosition));
                evt.menu.AppendAction("创建节点/表现节点/音效", _ => CreateNode(NodeType.SoundCue, graphMousePosition));
                evt.menu.AppendAction("创建节点/表现节点/飘字", _ => CreateNode(NodeType.FloatingTextCue, graphMousePosition));
            });

            return menuManipulator;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort =>
                endPort.direction != startPort.direction &&
                endPort.node != startPort.node
            ).ToList();
        }

        public SkillNodeBase CreateNode(NodeType nodeType, Vector2 position)
        {
            var node = NodeFactory.CreateNode(nodeType, position);
            if (node != null)
            {
                AddElement(node);
            }
            return node;
        }
    }
}
