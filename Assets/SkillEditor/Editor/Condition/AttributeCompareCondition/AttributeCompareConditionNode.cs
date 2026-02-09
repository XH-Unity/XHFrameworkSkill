using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 属性比较条件节点 - 比较目标属性值
    /// </summary>
    public class AttributeCompareConditionNode : ConditionNode<AttributeCompareConditionNodeData>
    {
        private EnumField attrTypeField;
        private EnumField compareOperatorField;
        private EnumField valueTypeField;
        private TextField compareValueField;
        private EnumField percentageBaseField;
        private VisualElement percentageBaseContainer;

        public AttributeCompareConditionNode(Vector2 position) : base(NodeType.AttributeCompareCondition, position) { }

        protected override string GetNodeTitle() => "属性比较";
        protected override float GetNodeWidth() => 200;

        protected override void CreateConditionContent()
        {
            // 属性类型
            attrTypeField = new EnumField("属性", AttrType.Health);
            ApplyFieldStyle(attrTypeField);
            attrTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.compareAttrType = (AttrType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(attrTypeField);

            // 比较操作符
            compareOperatorField = new EnumField("比较", CompareOperator.Less);
            ApplyFieldStyle(compareOperatorField);
            compareOperatorField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.compareOperator = (CompareOperator)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(compareOperatorField);

            // 值类型
            valueTypeField = new EnumField("值类型", AttributeValueType.Percentage);
            ApplyFieldStyle(valueTypeField);
            valueTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.compareValueType = (AttributeValueType)evt.newValue;
                    NotifyDataChanged();
                }
                OnValueTypeChanged((AttributeValueType)evt.newValue);
            });
            mainContainer.Add(valueTypeField);

            // 比较值
            compareValueField = CreateFormulaField("比较值", "30", value =>
            {
                if (TypedData != null)
                {
                    TypedData.compareValue = value;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(compareValueField);

            // 百分比基准属性容器
            percentageBaseContainer = new VisualElement();
            percentageBaseField = new EnumField("基准", AttrType.MaxHealth);
            ApplyFieldStyle(percentageBaseField);
            percentageBaseField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.percentageBaseAttrType = (AttrType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            percentageBaseContainer.Add(percentageBaseField);
            mainContainer.Add(percentageBaseContainer);

            // 默认显示百分比基准
            OnValueTypeChanged(AttributeValueType.Percentage);
        }

        private void OnValueTypeChanged(AttributeValueType valueType)
        {
            percentageBaseContainer.style.display = valueType == AttributeValueType.Percentage
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        }

        protected override void SyncConditionContentFromData()
        {
            if (TypedData == null) return;

            if (attrTypeField != null)
                attrTypeField.SetValueWithoutNotify(TypedData.compareAttrType);
            if (compareOperatorField != null)
                compareOperatorField.SetValueWithoutNotify(TypedData.compareOperator);
            if (valueTypeField != null)
            {
                valueTypeField.SetValueWithoutNotify(TypedData.compareValueType);
                OnValueTypeChanged(TypedData.compareValueType);
            }
            if (compareValueField != null)
                compareValueField.SetValueWithoutNotify(TypedData.compareValue ?? "30");
            if (percentageBaseField != null)
                percentageBaseField.SetValueWithoutNotify(TypedData.percentageBaseAttrType);
        }
    }
}
