using System;
using UnityEngine.Scripting.APIUpdating;

namespace SkillEditor.Data
{
    /// <summary>
    /// 属性比较条件节点数据
    /// 用于比较目标的属性值，根据结果执行不同的分支
    /// </summary>
    [Serializable]
    public class AttributeCompareConditionNodeData : ConditionNodeData
    {
        /// <summary>
        /// 要比较的属性类型
        /// </summary>
        public AttrType compareAttrType = AttrType.Health;

        /// <summary>
        /// 比较操作符
        /// </summary>
        public CompareOperator compareOperator = CompareOperator.Less;

        /// <summary>
        /// 比较值类型（固定值/百分比）
        /// </summary>
        public AttributeValueType compareValueType = AttributeValueType.Percentage;

        /// <summary>
        /// 比较值（支持公式）
        /// </summary>
        public string compareValue = "30";

        /// <summary>
        /// 百分比基准属性
        /// </summary>
        public AttrType percentageBaseAttrType = AttrType.MaxHealth;
    }
}
