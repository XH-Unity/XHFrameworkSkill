using System;
using System.Collections.Generic;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public static class NodeInspectorFactory
    {
        private static readonly List<(Type nodeType, Func<NodeInspectorBase> creator)> inspectorCreators = new()
        {
            // 技能节点
            (typeof(AbilityNode), () => new AbilityNodeInspector()),

            // ============ 效果节点 (Effect) ============

            // --- 瞬时效果 (Instant) ---
            (typeof(DamageEffectNode), () => new DamageEffectNodeInspector()),
            (typeof(HealEffectNode), () => new HealEffectNodeInspector()),
            (typeof(CostEffectNode), () => new CostEffectNodeInspector()),
            (typeof(ModifyAttributeEffectNode), () => new ModifyAttributeEffectNodeInspector()),
            (typeof(GenericEffectNode), () => new GenericEffectNodeInspector()),
            (typeof(ProjectileEffectNode), () => new ProjectileEffectNodeInspector()),
            (typeof(PlacementEffectNode), () => new PlacementEffectNodeInspector()),
            (typeof(DisplaceEffectNode), () => new DisplaceEffectNodeInspector()),

            // --- 持续效果 (Duration) ---
            (typeof(CooldownEffectNode), () => new CooldownEffectNodeInspector()),
            (typeof(BuffEffectNode), () => new BuffEffectNodeInspector()),

            // ============ 任务节点 (Task) ============
            (typeof(SearchTargetTaskNode), () => new SearchTargetTaskNodeInspector()),
            (typeof(EndAbilityTaskNode), () => new EndAbilityTaskNodeInspector()),

            // ============ 条件节点 (Condition) ============
            (typeof(AttributeCompareConditionNode), () => new AttributeCompareConditionNodeInspector()),

            // ============ 表现节点 (Cue) ============
            (typeof(ParticleCueNode), () => new ParticleCueNodeInspector()),
            (typeof(SoundCueNode), () => new SoundCueNodeInspector()),
            (typeof(FloatingTextCueNode), () => new FloatingTextCueNodeInspector()),
        };

        public static NodeInspectorBase CreateInspector(SkillNodeBase node)
        {
            if (node == null) return null;

            var nodeType = node.GetType();

            foreach (var (type, creator) in inspectorCreators)
            {
                if (type.IsAssignableFrom(nodeType))
                {
                    return creator();
                }
            }

            return new DefaultNodeInspector();
        }
    }
}
