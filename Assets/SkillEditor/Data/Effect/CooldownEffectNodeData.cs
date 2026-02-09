using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// CD类型
    /// </summary>
    public enum CooldownType
    {
        [InspectorName("普通CD")]
        Normal,
        [InspectorName("充能CD")]
        Charge
    }

    /// <summary>
    /// 冷却效果节点数据
    /// 支持普通CD和充能CD两种模式
    /// </summary>
    [Serializable]
    public class CooldownEffectNodeData : EffectNodeData
    {
        /// <summary>
        /// CD类型
        /// </summary>
        public CooldownType cooldownType = CooldownType.Normal;

        // ============ 充能CD参数 ============

        /// <summary>
        /// 最大充能数（充能CD模式）
        /// </summary>
        public int maxCharges = 2;

        /// <summary>
        /// 每层充能恢复时间（充能CD模式）
        /// </summary>
        public string chargeTime = "10";

        public CooldownEffectNodeData()
        {
            // 普通CD是持续类型
            durationType = EffectDurationType.Duration;

            // CD不随技能结束而取消，由ASC管理生命周期
            cancelOnAbilityEnd = false;
        }
    }
}
