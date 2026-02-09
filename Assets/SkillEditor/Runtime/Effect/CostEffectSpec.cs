using SkillEditor.Data;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 消耗效果Spec（瞬时效果）
    /// </summary>
    public class CostEffectSpec : GameplayEffectSpec
    {
        private CostEffectNodeData CostNodeData => NodeData as CostEffectNodeData;
    }
}
