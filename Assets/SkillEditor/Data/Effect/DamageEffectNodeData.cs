using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 伤害效果节点数据（瞬时效果）
    /// </summary>
    [Serializable]
    public class DamageEffectNodeData : EffectNodeData
    {
        /// <summary>
        /// 伤害类型
        /// </summary>
        public DamageType damageType = DamageType.Physical;

        /// <summary>
        /// 伤害值来源类型
        /// </summary>
        public ModifierMagnitudeSourceType damageSourceType = ModifierMagnitudeSourceType.FixedValue;

        /// <summary>
        /// 具体伤害值（当 damageSourceType = FixedValue 时使用）
        /// </summary>
        public float damageFixedValue = 10f;

        /// <summary>
        /// 伤害公式（当 damageSourceType = Formula 时使用）
        /// </summary>
        public string damageFormula = "";

        /// <summary>
        /// MMC 类型（当 damageSourceType = ModifierMagnitudeCalculation 时使用）
        /// </summary>
        public MMCType damageMMCType = MMCType.AttributeBased;

        /// <summary>
        /// 上下文数据键名（当 damageSourceType = SetByCaller 时使用）
        /// </summary>
        public string damageSetByCallerKey = "";

        // ============ MMC 详细配置 ============

        /// <summary>
        /// MMC 捕获的属性类型
        /// </summary>
        public AttrType damageMMCCaptureAttribute = AttrType.Attack;

        /// <summary>
        /// MMC 属性来源
        /// </summary>
        public MMCAttributeSource damageMMCAttributeSource = MMCAttributeSource.Source;

        /// <summary>
        /// MMC 系数
        /// </summary>
        public float damageMMCCoefficient = 1f;

        /// <summary>
        /// MMC 是否使用快照
        /// </summary>
        public bool damageMMCUseSnapshot = true;

        /// <summary>
        /// 是否乘以堆叠层数（当被 Buff 触发时，伤害 × Buff层数）
        /// </summary>
        public bool damageMultiplyByStackCount = false;

        /// <summary>
        /// 伤害计算类型
        /// </summary>
        public DamageCalculationType damageCalculationType = DamageCalculationType.Default;
    }
}
