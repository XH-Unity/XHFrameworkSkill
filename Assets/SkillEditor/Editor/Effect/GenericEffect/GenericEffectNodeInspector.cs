using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 通用效果节点Inspector - 无额外字段，全部由基类处理
    /// </summary>
    public class GenericEffectNodeInspector : EffectNodeInspector
    {
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => true;
        protected override bool ShowAttributeModifiers => true;
        protected override bool ShowStackConfig => true;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
        }
    }
}
