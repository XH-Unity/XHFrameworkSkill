using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class DamageEffectNodeInspector : EffectNodeInspector
    {
        protected override bool ShowAttributeModifiers => true;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is DamageEffectNode damageNode)
            {
                var data = damageNode.TypedData;
                if (data == null) return;

                // 伤害类型
                var damageTypeField = new EnumField("伤害类型", data.damageType) { style = { marginBottom = 8 } };
                ApplyEnumFieldStyle(damageTypeField);
                damageTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.damageType = (DamageType)evt.newValue;
                    damageNode.SyncUIFromData();
                });
                container.Add(damageTypeField);

                // 伤害值 - 使用四选项系统
                container.Add(CreateMagnitudeSourceUIWithMMCDetail(
                    "伤害值",
                    data,
                    damageNode
                ));

                // 伤害计算类型
                var calcTypeField = new EnumField("计算方式", data.damageCalculationType) { style = { marginTop = 4 } };
                ApplyEnumFieldStyle(calcTypeField);
                calcTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.damageCalculationType = (DamageCalculationType)evt.newValue;
                    damageNode.SyncUIFromData();
                });
                container.Add(calcTypeField);
            }
        }

        /// <summary>
        /// 创建带 MMC 详细配置的数值来源 UI
        /// </summary>
        private VisualElement CreateMagnitudeSourceUIWithMMCDetail(string label, DamageEffectNodeData data, DamageEffectNode node)
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
            var sourceTypeField = new EnumField(data.damageSourceType);
            sourceTypeField.style.width = 100;
            sourceTypeField.style.marginRight = 4;
            ApplyEnumFieldStyle(sourceTypeField);
            valueRow.Add(sourceTypeField);

            // ===== 具体值输入框 =====
            var fixedValueField = new FloatField { value = data.damageFixedValue };
            fixedValueField.style.flexGrow = 1;
            fixedValueField.style.display = data.damageSourceType == ModifierMagnitudeSourceType.FixedValue
                ? DisplayStyle.Flex : DisplayStyle.None;
            fixedValueField.RegisterValueChangedCallback(evt =>
            {
                data.damageFixedValue = evt.newValue;
                node.SyncUIFromData();
            });
            valueRow.Add(fixedValueField);

            // ===== 公式输入框 =====
            var formulaField = new TextField { value = data.damageFormula ?? "" };
            formulaField.style.flexGrow = 1;
            formulaField.style.display = data.damageSourceType == ModifierMagnitudeSourceType.Formula
                ? DisplayStyle.Flex : DisplayStyle.None;
            formulaField.RegisterValueChangedCallback(evt =>
            {
                data.damageFormula = evt.newValue;
                node.SyncUIFromData();
            });
            valueRow.Add(formulaField);

            // ===== MMC 类型枚举选择 =====
            var mmcTypeField = new EnumField(data.damageMMCType);
            mmcTypeField.style.flexGrow = 1;
            mmcTypeField.style.display = data.damageSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                ? DisplayStyle.Flex : DisplayStyle.None;
            ApplyEnumFieldStyle(mmcTypeField);
            valueRow.Add(mmcTypeField);

            // ===== 上下文数据键名输入框 =====
            var setByCallerField = new TextField { value = data.damageSetByCallerKey ?? "" };
            setByCallerField.style.flexGrow = 1;
            setByCallerField.style.display = data.damageSourceType == ModifierMagnitudeSourceType.SetByCaller
                ? DisplayStyle.Flex : DisplayStyle.None;
            setByCallerField.RegisterValueChangedCallback(evt =>
            {
                data.damageSetByCallerKey = evt.newValue;
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
            mmcDetailContainer.style.borderLeftColor = new Color(0.3f, 0.6f, 0.9f);
            mmcDetailContainer.style.display = (data.damageSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                && data.damageMMCType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

            // MMC 捕获属性
            var mmcCaptureAttrField = new AttributeField("捕获属性");
            mmcCaptureAttrField.Value = data.damageMMCCaptureAttribute;
            mmcCaptureAttrField.OnValueChanged += value =>
            {
                data.damageMMCCaptureAttribute = value;
                node.SyncUIFromData();
            };
            mmcDetailContainer.Add(mmcCaptureAttrField);

            // MMC 属性来源
            var mmcSourceField = new EnumField("属性来源", data.damageMMCAttributeSource);
            mmcSourceField.style.marginBottom = 4;
            ApplyEnumFieldStyle(mmcSourceField);
            mmcSourceField.RegisterValueChangedCallback(evt =>
            {
                data.damageMMCAttributeSource = (MMCAttributeSource)evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcSourceField);

            // MMC 系数
            var mmcCoefficientField = new FloatField("系数") { value = data.damageMMCCoefficient };
            mmcCoefficientField.style.marginBottom = 4;
            mmcCoefficientField.RegisterValueChangedCallback(evt =>
            {
                data.damageMMCCoefficient = evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcCoefficientField);

            // MMC 快照模式
            var mmcSnapshotToggle = new Toggle("使用快照（施放时捕获）") { value = data.damageMMCUseSnapshot };
            mmcSnapshotToggle.tooltip = "勾选：施放时捕获属性值，后续不变\n不勾选：每次计算时实时读取属性值";
            mmcSnapshotToggle.RegisterValueChangedCallback(evt =>
            {
                data.damageMMCUseSnapshot = evt.newValue;
                node.SyncUIFromData();
            });
            mmcDetailContainer.Add(mmcSnapshotToggle);

            container.Add(mmcDetailContainer);

            // ===== 乘以堆叠层数选项 =====
            var stackMultiplyToggle = new Toggle("乘以堆叠层数") { value = data.damageMultiplyByStackCount };
            stackMultiplyToggle.tooltip = "勾选后，当此伤害被 Buff 的周期效果触发时，伤害会乘以 Buff 的堆叠层数\n例如：基础伤害 50，Buff 3层 → 实际伤害 150";
            stackMultiplyToggle.style.marginTop = 8;
            stackMultiplyToggle.RegisterValueChangedCallback(evt =>
            {
                data.damageMultiplyByStackCount = evt.newValue;
                node.SyncUIFromData();
            });
            container.Add(stackMultiplyToggle);

            // 数值来源类型切换事件
            sourceTypeField.RegisterValueChangedCallback(evt =>
            {
                var newType = (ModifierMagnitudeSourceType)evt.newValue;
                data.damageSourceType = newType;

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
                    && data.damageMMCType == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;

                node.SyncUIFromData();
            });

            // MMC 类型切换事件
            mmcTypeField.RegisterValueChangedCallback(evt =>
            {
                data.damageMMCType = (MMCType)evt.newValue;
                mmcDetailContainer.style.display = (data.damageSourceType == ModifierMagnitudeSourceType.ModifierMagnitudeCalculation
                    && (MMCType)evt.newValue == MMCType.AttributeBased) ? DisplayStyle.Flex : DisplayStyle.None;
                node.SyncUIFromData();
            });

            return container;
        }
    }
}
