using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class HealEffectNodeInspector : EffectNodeInspector
    {
        protected override bool ShowAttributeModifiers => false;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is HealEffectNode healNode)
            {
                var data = healNode.TypedData;
                if (data == null) return;

                // 治疗值 - 使用带 MMC 详细配置的 UI
                container.Add(CreateMagnitudeSourceUIWithMMCDetail(
                    "治疗值",
                    data,
                    healNode
                ));

                // 治疗计算类型
                var calcTypeField = new EnumField("计算方式", data.healCalculationType) { style = { marginTop = 4 } };
                ApplyEnumFieldStyle(calcTypeField);
                calcTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.healCalculationType = (HealCalculationType)evt.newValue;
                    healNode.SyncUIFromData();
                });
                container.Add(calcTypeField);
            }
        }

        /// <summary>
        /// 创建带 MMC 详细配置的数值来源 UI
        /// </summary>
        private VisualElement CreateMagnitudeSourceUIWithMMCDetail(string label, HealEffectNodeData data, HealEffectNode node)
        {
            var container = new VisualElement();
            container.style.marginBottom = 8;

            // 标签行
            var labelElement = new Label(label);
            labelElement.style.marginBottom = 4;
            container.Add(labelElement);

            // 数值行：来源类型下拉框 + 输入框
            var valueRow = new VisualElement();
            valueRow.style.flexDirection = FlexDirection.Row;

            // 数值来源类型下拉框
            var sourceTypeField = new EnumField(data.healSourceType);
            sourceTypeField.style.width = 100;
            sourceTypeField.style.marginRight = 4;
            ApplyEnumFieldStyle(sourceTypeField);
            valueRow.Add(sourceTypeField);

            // ===== 具体值输入框 =====
            var fixedValueField = new FloatField { value = data.healFixedValue };
            fixedValueField.style.flexGrow = 1;
            fixedValueField.style.display = data.healSourceType == ModifierMagnitudeSourceType.FixedValue
                ? DisplayStyle.Flex : DisplayStyle.None;
            fixedValueField.RegisterValueChangedCallback(evt =>
            {
                data.healFixedValue = evt.newValue;
                node.SyncUIFromData();
            });
            valueRow.Add(fixedValueField);

            // ===== 公式输入框 =====
            var formulaField = new TextField { value = data.healFormula ?? "" };
            formulaField.style.flexGrow = 1;
            formulaField.style.display = data.healSourceType == ModifierMagnitudeSourceType.Formula
                ? DisplayStyle.Flex : DisplayStyle.None;
            formulaField.RegisterValueChangedCallback(evt =>
            {
                data.healFormula = evt.newValue;
                node.SyncUIFromData();
            });
            valueRow.Add(formulaField);

            // ===== MMC 类型枚举选择 =====
            var mmcTypeField = new EnumField(data.healMMCType);
            mmcTypeField.style.flexGrow = 1;
            mmcTypeField.style.display = data.healSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyEnumFieldStyle(mmcTypeField);
            valueRow.Add(mmcTypeField);

            // ===== 上下文数据键名输入框 =====
            var setByCallerField = new TextField { value = data.healSetByCallerKey ?? "" };
            setByCallerField.style.flexGrow = 1;
            setByCallerField.style.display = data.healSourceType == ModifierMagnitudeSourceType.SetByCaller
                ? DisplayStyle.Flex : DisplayStyle.None;
            setByCallerField.RegisterValueChangedCallback(evt =>
            {
                data.healSetByCallerKey = evt.newValue;
                node.SyncUIFromData();
            });
            valueRow.Add(setByCallerField);

            container.Add(valueRow);

            // ===== MMC 详细配置容器 =====
            var mmcDetailContainer = new VisualElement();
            mmcDetailContainer.style.marginTop = 4;
            mmcDetailContainer.style.marginLeft = 8;
            mmcDetailContainer.style.paddingLeft = 8;
            mmcDetailContainer.style.borderLeftWidth = 2;
            mmcDetailContainer.style.borderLeftColor = new Color(0.3f, 0.8f, 0.3f);  // 绿色，区分治疗
            mmcDetailContainer.style.display = (data.healSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                && data.healMMCType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

            // MMC 捕获属性
            var mmcCaptureAttrField = new AttributeField("捕获属性");
            mmcCaptureAttrField.Value = data.healMMCCaptureAttribute;
            mmcCaptureAttrField.OnValueChanged += value =>
            {
                data.healMMCCaptureAttribute = value;
                node.SyncUIFromData();
            };
            mmcDetailContainer.Add(mmcCaptureAttrField);

            // MMC 属性来源
            var mmcSourceField = new EnumField("属性来源", data.healMMCAttributeSource);
            mmcSourceField.style.marginBottom = 4;
            ApplyEnumFieldStyle(mmcSourceField);
            mmcSourceField.RegisterValueChangedCallback(evt =>
            {
                data.healMMCAttributeSource = (MMCAttributeSource)evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcSourceField);

            // MMC 系数
            var mmcCoefficientField = new FloatField("系数") { value = data.healMMCCoefficient };
            mmcCoefficientField.style.marginBottom = 4;
            mmcCoefficientField.RegisterValueChangedCallback(evt =>
            {
                data.healMMCCoefficient = evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcCoefficientField);

            // MMC 快照模式
            var mmcSnapshotToggle = new Toggle("使用快照（施放时捕获）") { value = data.healMMCUseSnapshot };
            mmcSnapshotToggle.tooltip = "勾选：施放时捕获属性值，后续不变\n不勾选：每次计算时实时读取属性值";
            mmcSnapshotToggle.RegisterValueChangedCallback(evt =>
            {
                data.healMMCUseSnapshot = evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcSnapshotToggle);

            container.Add(mmcDetailContainer);

            // ===== 乘以堆叠层数选项 =====
            var stackMultiplyToggle = new Toggle("乘以堆叠层数") { value = data.healMultiplyByStackCount };
            stackMultiplyToggle.tooltip = "勾选后，当此治疗被 Buff 的周期效果触发时，治疗量会乘以 Buff 的堆叠层数\n例如：基础治疗 50，Buff 3层 → 实际治疗 150";
            stackMultiplyToggle.style.marginTop = 8;
            stackMultiplyToggle.RegisterValueChangedCallback(evt =>
            {
                data.healMultiplyByStackCount = evt.newValue;
                node.SyncUIFromData();
            });
            container.Add(stackMultiplyToggle);

            // 数值来源类型切换事件
            sourceTypeField.RegisterValueChangedCallback(evt =>
            {
                var newType = (ModifierMagnitudeSourceType)evt.newValue;
                data.healSourceType = newType;

                fixedValueField.style.display = newType == ModifierMagnitudeSourceType.FixedValue
                    ? DisplayStyle.Flex : DisplayStyle.None;
                formulaField.style.display = newType == ModifierMagnitudeSourceType.Formula
                    ? DisplayStyle.Flex : DisplayStyle.None;
                mmcTypeField.style.display = newType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    ? DisplayStyle.Flex : DisplayStyle.None;
                setByCallerField.style.display = newType == ModifierMagnitudeSourceType.SetByCaller
                    ? DisplayStyle.Flex : DisplayStyle.None;

                // 更新 MMC 详细配置显示
                mmcDetailContainer.style.display = (newType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    && data.healMMCType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

                node.SyncUIFromData();
            });

            // MMC 类型切换事件
            mmcTypeField.RegisterValueChangedCallback(evt =>
            {
                data.healMMCType = (MMCType)evt.newValue;
                mmcDetailContainer.style.display = (data.healSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    && (MMCType)evt.newValue == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;
                node.SyncUIFromData();
            });

            return container;
        }
    }
}
