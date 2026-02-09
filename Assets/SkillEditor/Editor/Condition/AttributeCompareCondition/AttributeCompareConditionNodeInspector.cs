using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 属性比较条件节点Inspector
    /// </summary>
    public class AttributeCompareConditionNodeInspector : ConditionNodeInspector
    {
        protected override void BuildConditionInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is AttributeCompareConditionNode compareNode)
            {
                var data = compareNode.TypedData;
                if (data == null) return;

                // 属性类型
                var attrField = new AttributeField("比较属性");
                attrField.Value = data.compareAttrType;
                attrField.OnValueChanged += value =>
                {
                    data.compareAttrType = value;
                    compareNode.SyncUIFromData();
                };
                container.Add(attrField);

                // 比较操作符
                var compareOpField = new EnumField("比较操作", data.compareOperator);
                ApplyEnumFieldStyle(compareOpField);
                compareOpField.RegisterValueChangedCallback(evt =>
                {
                    data.compareOperator = (CompareOperator)evt.newValue;
                    compareNode.SyncUIFromData();
                });
                container.Add(compareOpField);

                // 值类型
                var valueTypeField = new EnumField("值类型", data.compareValueType);
                ApplyEnumFieldStyle(valueTypeField);
                container.Add(valueTypeField);

                // 比较值
                var compareValueField = CreateFormulaField("比较值", data.compareValue ?? "30", value =>
                {
                    data.compareValue = value;
                    compareNode.SyncUIFromData();
                });
                container.Add(compareValueField);

                // 百分比基准属性
                var percentageBaseField = new AttributeField("百分比基准");
                percentageBaseField.Value = data.percentageBaseAttrType;
                percentageBaseField.OnValueChanged += value =>
                {
                    data.percentageBaseAttrType = value;
                    compareNode.SyncUIFromData();
                };
                percentageBaseField.style.display = data.compareValueType == AttributeValueType.Percentage
                    ? DisplayStyle.Flex : DisplayStyle.None;
                container.Add(percentageBaseField);

                // 值类型切换事件
                valueTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.compareValueType = (AttributeValueType)evt.newValue;
                    percentageBaseField.style.display = data.compareValueType == AttributeValueType.Percentage
                        ? DisplayStyle.Flex : DisplayStyle.None;
                    compareNode.SyncUIFromData();
                });
            }
        }
    }
}
