using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// Buff效果Spec（持续效果）
    /// </summary>
    public class BuffEffectSpec : GameplayEffectSpec
    {
        private BuffEffectNodeData BuffNodeData => NodeData as BuffEffectNodeData;

        protected override SpecExecutionContext GetExecutionContext()
        {
            // Buff 的目标就是 Target（Buff 持有者）
            var currentTarget = Target ?? Context?.GetTarget(NodeData?.targetType ?? Data.TargetType.ParentInput);

            return new SpecExecutionContext
            {
                AbilitySpec = Context?.AbilitySpec,
                OwnerEffectSpec = this,
                Caster = Context?.Caster,
                MainTarget = Context?.MainTarget,
                ParentInputTarget = currentTarget,  // 将 Buff 的目标作为 ParentInputTarget 传递
                AbilityLevel = Context?.AbilityLevel ?? 1,
                StackCount = this.StackCount  // 传递 Buff 的堆叠层数
            };
        }


    }
}
