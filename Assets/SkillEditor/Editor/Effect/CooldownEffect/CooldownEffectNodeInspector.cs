using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 冷却效果节点Inspector
    /// 支持普通CD和充能CD两种模式
    /// </summary>
    public class CooldownEffectNodeInspector : EffectNodeInspector
    {
        // 冷却不需要显示节点目标
        protected override bool ShowTargetType => false;

        // 我们自己处理持续时间显示
        protected override bool ShowDurationConfig => false;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is CooldownEffectNode cdNode)
            {
                var data = cdNode.TypedData;
                if (data == null) return;

                // CD类型
                var cdTypeField = new EnumField("CD类型", data.cooldownType);
                ApplyEnumFieldStyle(cdTypeField);
                container.Add(cdTypeField);

                // 普通CD参数容器
                var normalContainer = new VisualElement();
                normalContainer.Add(CreateFormulaField("持续时间(秒)", data.duration ?? "2", value =>
                {
                    data.duration = value;
                    cdNode.SyncUIFromData();
                }));
                container.Add(normalContainer);

                // 充能CD参数容器
                var chargeContainer = new VisualElement();
                chargeContainer.Add(CreateIntField("最大充能", data.maxCharges, value =>
                {
                    data.maxCharges = value;
                    cdNode.SyncUIFromData();
                }));
                chargeContainer.Add(CreateFormulaField("充能时间(秒)", data.chargeTime ?? "10", value =>
                {
                    data.chargeTime = value;
                    cdNode.SyncUIFromData();
                }));
                container.Add(chargeContainer);

                // 根据类型显示/隐藏
                void UpdateDisplay(CooldownType type)
                {
                    normalContainer.style.display = type == CooldownType.Normal
                        ? DisplayStyle.Flex : DisplayStyle.None;
                    chargeContainer.style.display = type == CooldownType.Charge
                        ? DisplayStyle.Flex : DisplayStyle.None;
                }

                cdTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.cooldownType = (CooldownType)evt.newValue;
                    UpdateDisplay((CooldownType)evt.newValue);
                    cdNode.SyncUIFromData();
                });

                UpdateDisplay(data.cooldownType);
            }
        }
    }
}
