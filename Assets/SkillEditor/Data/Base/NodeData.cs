using System;
using System.Collections.Generic;
using UnityEngine;


namespace SkillEditor.Data
{
    /// <summary>
    /// 属性修改器数据 - 对应GAS的GameplayModifierInfo
    /// </summary>
    [Serializable]
    public class AttributeModifierData
    {
        /// <summary>
        /// 目标属性类型
        /// </summary>
        public AttrType attrType = AttrType.None;

        /// <summary>
        /// 修改操作类型
        /// </summary>
        public ModifierOperation operation = ModifierOperation.Add;

        /// <summary>
        /// 数值来源类型
        /// </summary>
        public ModifierMagnitudeSourceType magnitudeSourceType = ModifierMagnitudeSourceType.FixedValue;

        /// <summary>
        /// 具体值（当 magnitudeSourceType = FixedValue 时使用）
        /// </summary>
        public float fixedValue = 0f;

        /// <summary>
        /// 公式（当 magnitudeSourceType = Formula 时使用）
        /// 如 "Caster.Attack * 0.5 + 10"
        /// </summary>
        public string formula = "";

        /// <summary>
        /// MMC 类型（当 magnitudeSourceType = ModifierMagnitudeCalculation 时使用）
        /// </summary>
        public MMCType mmcType = MMCType.AttributeBased;

        /// <summary>
        /// 上下文数据键名（当 magnitudeSourceType = SetByCaller 时使用）
        /// </summary>
        public string setByCallerKey = "";

        // ============ MMC 详细配置（当 mmcType = AttributeBased 时使用）============

        /// <summary>
        /// MMC 捕获的属性类型
        /// </summary>
        public AttrType mmcCaptureAttribute = AttrType.Attack;

        /// <summary>
        /// MMC 属性来源（从谁身上读取属性）
        /// </summary>
        public MMCAttributeSource mmcAttributeSource = MMCAttributeSource.Source;

        /// <summary>
        /// MMC 系数（属性值 × 系数 = 最终值）
        /// </summary>
        public float mmcCoefficient = 1f;

        /// <summary>
        /// MMC 是否使用快照（true=施放时捕获，false=实时计算）
        /// </summary>
        public bool mmcUseSnapshot = true;
    }

    /// <summary>
    /// MMC 属性来源 - 从谁身上读取属性
    /// </summary>
    public enum MMCAttributeSource
    {
        [InspectorName("施法者")]
        Source,
        [InspectorName("目标")]
        Target
    }

    /// <summary>
    /// 节点序列化数据基类
    /// </summary>
    [Serializable]
    public class NodeData
    {
        // ============ 基础字段 ============
        public string guid;
        public NodeType nodeType;
        public Vector2 position;

        // ============ 通用字段 ============
        public TargetType targetType = TargetType.Caster;
    }

    /// <summary>
    /// 连接数据 - 保存节点之间的连线信息
    /// </summary>
    [Serializable]
    public class ConnectionData
    {
        public string outputNodeGuid;
        public string outputPortName;
        public string inputNodeGuid;
        public string inputPortName;
    }
}
