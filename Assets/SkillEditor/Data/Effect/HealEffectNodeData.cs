using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 治疗效果节点数据（瞬时效果）
    /// </summary>
    [Serializable]
    public class HealEffectNodeData : EffectNodeData
    {
        /// <summary>
        /// 治疗值来源类型
        /// </summary>
        public ModifierMagnitudeSourceType healSourceType = ModifierMagnitudeSourceType.FixedValue;

        /// <summary>
        /// 具体治疗值
        /// </summary>
        public float healFixedValue = 10f;

        /// <summary>
        /// 治疗公式
        /// </summary>
        public string healFormula = "";

        /// <summary>
        /// MMC 类型
        /// </summary>
        public MMCType healMMCType = MMCType.AttributeBased;

        /// <summary>
        /// 上下文数据键名
        /// </summary>
        public string healSetByCallerKey = "";

        // ============ MMC 详细配置 ============

        /// <summary>
        /// MMC 捕获的属性类型
        /// </summary>
        public AttrType healMMCCaptureAttribute = AttrType.MagicPower;

        /// <summary>
        /// MMC 属性来源
        /// </summary>
        public MMCAttributeSource healMMCAttributeSource = MMCAttributeSource.Source;

        /// <summary>
        /// MMC 系数
        /// </summary>
        public float healMMCCoefficient = 1f;

        /// <summary>
        /// MMC 是否使用快照
        /// </summary>
        public bool healMMCUseSnapshot = true;

        /// <summary>
        /// 是否乘以堆叠层数（当被 Buff 触发时，治疗量 × Buff层数）
        /// </summary>
        public bool healMultiplyByStackCount = false;

        /// <summary>
        /// 治疗计算类型
        /// </summary>
        public HealCalculationType healCalculationType = HealCalculationType.Default;
    }
}
