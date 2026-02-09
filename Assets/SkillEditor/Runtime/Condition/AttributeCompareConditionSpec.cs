using SkillEditor.Data;
using SkillEditor.Runtime.Utils;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 属性比较条件Spec
    /// </summary>
    public class AttributeCompareConditionSpec : ConditionSpec
    {
        private AttributeCompareConditionNodeData CompareNodeData => NodeData as AttributeCompareConditionNodeData;

        protected override bool Evaluate(AbilitySystemComponent target)
        {
            var nodeData = CompareNodeData;

            if (target?.Attributes == null || nodeData == null)
                return false;

            float? attrValue = target.Attributes.GetCurrentValue(nodeData.compareAttrType);
            if (!attrValue.HasValue)
                return false;

            float compareValue = FormulaEvaluator.EvaluateSimple(nodeData.compareValue, 0f);
            if (nodeData.compareValueType == AttributeValueType.Percentage)
            {
                float? baseValue = target.Attributes.GetCurrentValue(nodeData.percentageBaseAttrType);
                if (baseValue.HasValue)
                    compareValue = baseValue.Value * (compareValue / 100f);
            }

            return nodeData.compareOperator switch
            {
                CompareOperator.Equal => System.Math.Abs(attrValue.Value - compareValue) < 0.0001f,
                CompareOperator.NotEqual => System.Math.Abs(attrValue.Value - compareValue) >= 0.0001f,
                CompareOperator.Greater => attrValue.Value > compareValue,
                CompareOperator.GreaterOrEqual => attrValue.Value >= compareValue,
                CompareOperator.Less => attrValue.Value < compareValue,
                CompareOperator.LessOrEqual => attrValue.Value <= compareValue,
                _ => false
            };
        }
    }
}
