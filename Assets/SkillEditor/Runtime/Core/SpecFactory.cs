using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Spec工厂 - 根据节点类型创建对应的Spec实例
    /// 重构后支持 Effect、Task、Condition 三种类型
    /// </summary>
    public static class SpecFactory
    {
        /// <summary>
        /// 创建效果Spec
        /// </summary>
        public static GameplayEffectSpec CreateEffectSpec(NodeType nodeType)
        {
            switch (nodeType)
            {
                // ============ 瞬时效果 (Instant) ============

                // 伤害/治疗
                case NodeType.DamageEffect:
                    return new DamageEffectSpec();
                case NodeType.HealEffect:
                    return new HealEffectSpec();

                // 消耗
                case NodeType.CostEffect:
                    return new CostEffectSpec();

                // 属性修改
                case NodeType.ModifyAttributeEffect:
                    return new ModifyAttributeEffectSpec();

                // 通用效果
                case NodeType.GenericEffect:
                    return new GenericEffectSpec();

                // 投射物
                case NodeType.ProjectileEffect:
                    return new ProjectileEffectSpec();

                // 放置物
                case NodeType.PlacementEffect:
                    return new PlacementEffectSpec();

                // 位移
                case NodeType.DisplaceEffect:
                    return new DisplaceEffectSpec();

                // ============ 持续效果 (Duration) ============

                // 冷却
                case NodeType.CooldownEffect:
                    return new CooldownEffectSpec();

                // Buff效果
                case NodeType.BuffEffect:
                    return new BuffEffectSpec();

                default:
                    return null;
            }
        }

        /// <summary>
        /// 创建任务Spec
        /// </summary>
        public static TaskSpec CreateTaskSpec(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.SearchTargetTask:
                    return new SearchTargetTaskSpec();

                case NodeType.EndAbilityTask:
                    return new EndAbilityTaskSpec();

                default:
                    return null;
            }
        }

        /// <summary>
        /// 创建条件Spec
        /// </summary>
        public static ConditionSpec CreateConditionSpec(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.AttributeCompareCondition:
                    return new AttributeCompareConditionSpec();

                default:
                    return null;
            }
        }

        /// <summary>
        /// 创建Cue Spec
        /// </summary>
        public static GameplayCueSpec CreateCueSpec(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.ParticleCue:
                    return new ParticleCueSpec();

                case NodeType.SoundCue:
                    return new SoundCueSpec();

                case NodeType.FloatingTextCue:
                    return new FloatingTextCueSpec();

                default:
                    return null;
            }
        }

        /// <summary>
        /// 判断节点类型是否为瞬时效果
        /// </summary>
        public static bool IsInstantEffect(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.DamageEffect:
                case NodeType.HealEffect:
                case NodeType.CostEffect:
                case NodeType.ModifyAttributeEffect:
                case NodeType.ProjectileEffect:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断节点类型是否为持续效果
        /// </summary>
        public static bool IsDurationEffect(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.CooldownEffect:
                case NodeType.BuffEffect:
                case NodeType.PlacementEffect:
                case NodeType.DisplaceEffect:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断节点类型是否为任务节点
        /// </summary>
        public static bool IsTaskNode(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.SearchTargetTask:
                case NodeType.EndAbilityTask:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// 判断节点类型是否为条件节点
        /// </summary>
        public static bool IsConditionNode(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.AttributeCompareCondition:
                    return true;
                default:
                    return false;
            }
        }
    }
}
