using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// Buff效果节点数据（统一的Buff类型）
    /// 合并了原来的 DurationBuffEffectNodeData 和 PeriodicBuffEffectNodeData
    /// 通过 isPeriodic 属性区分是否为周期Buff
    /// </summary>
    [Serializable]
    public class BuffEffectNodeData : EffectNodeData
    {
        /// <summary>
        /// Buff ID（用于查找和标识）
        /// </summary>
        public int buffId = 0;

        public BuffEffectNodeData()
        {
            // Buff默认配置
            durationType = EffectDurationType.Duration;
            duration = "10";
            stackType = StackType.AggregateBySource;
            stackLimit = 5;

            // 周期配置默认关闭，用户可以在编辑器中开启
            isPeriodic = false;
            period = "1";
            executeOnApplication = false;

            // Buff不随技能结束而取消，由ASC管理生命周期
            cancelOnAbilityEnd = false;
        }
    }
}
