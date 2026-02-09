using System;
using System.Collections.Generic;

namespace SkillEditor.Data
{
    /// <summary>
    /// 时间效果数据 - 只有触发时间，没有结束时间
    /// </summary>
    [Serializable]
    public class TimeEffectData
    {
        /// <summary>
        /// 触发帧
        /// </summary>
        public int triggerTime = 0;

        /// <summary>
        /// 端口唯一标识（用于连线恢复）
        /// </summary>
        public string portId;

        public TimeEffectData()
        {
            portId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 时间Cue数据 - 有开始时间和结束时间，用于控制Cue的生命周期
    /// </summary>
    [Serializable]
    public class TimeCueData
    {
        /// <summary>
        /// 开始帧
        /// </summary>
        public int startTime = 0;

        /// <summary>
        /// 结束帧，-1表示动画结束时自动结束
        /// </summary>
        public int endTime = 5;

        /// <summary>
        /// 端口唯一标识（用于连线恢复）
        /// </summary>
        public string portId;

        public TimeCueData()
        {
            portId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// 技能节点数据 - 对应 GameplayAbility
    /// </summary>
    [Serializable]
    public class AbilityNodeData : NodeData
    {
        /// <summary>
        /// 技能ID（用于查找和标识）
        /// </summary>
        public int skillId = 0;

        // ============ 技能标签配置 ============

        /// <summary>
        /// 技能自身标签 - 用于标识此技能
        /// </summary>
        public GameplayTagSet assetTags;

        /// <summary>
        /// 取消带有这些标签的技能
        /// </summary>
        public GameplayTagSet cancelAbilitiesWithTags;

        /// <summary>
        /// 阻止带有这些标签的技能激活
        /// </summary>
        public GameplayTagSet blockAbilitiesWithTags;

        /// <summary>
        /// 激活时授予的标签
        /// </summary>
        public GameplayTagSet activationOwnedTags;

        /// <summary>
        /// 激活所需标签 - 拥有者必须拥有这些标签才能激活
        /// </summary>
        public GameplayTagSet activationRequiredTags;

        /// <summary>
        /// 激活阻止标签 - 拥有者拥有这些标签时阻止激活
        /// </summary>
        public GameplayTagSet activationBlockedTags;

        /// <summary>
        /// 运行时阻止标签 - 技能运行期间如果获得这些标签，技能被取消
        /// 用于眩晕、沉默等控制效果打断技能
        /// </summary>
        public GameplayTagSet ongoingBlockedTags;

        // ============ 事件监听配置 ============

        /// <summary>
        /// 事件输出端口（用于被动技能等事件监听）
        /// </summary>
        public List<AbilityEventPortData> eventOutputPorts = new List<AbilityEventPortData>();
    }

    /// <summary>
    /// 技能事件端口数据 - 用于被动技能等事件监听
    /// </summary>
    [Serializable]
    public class AbilityEventPortData
    {
        public GameplayEventType eventType = GameplayEventType.OnHit;
        public string PortId { get; set; }
        public string customEventTag = "";  // 当eventType为Custom时使用
    }

    public struct AbilityTagContainer
    {
        public GameplayTagSet AssetTags;
        public GameplayTagSet CancelAbilitiesWithTags;
        public GameplayTagSet BlockAbilitiesWithTags;
        public GameplayTagSet ActivationOwnedTags;
        public GameplayTagSet ActivationRequiredTags;
        public GameplayTagSet ActivationBlockedTags;
        public GameplayTagSet OngoingBlockedTags;

        public AbilityTagContainer(AbilityNodeData data)
        {
            AssetTags = data.assetTags;
            CancelAbilitiesWithTags = data.cancelAbilitiesWithTags;
            BlockAbilitiesWithTags = data.blockAbilitiesWithTags;
            ActivationOwnedTags = data.activationOwnedTags;
            ActivationRequiredTags = data.activationRequiredTags;
            ActivationBlockedTags = data.activationBlockedTags;
            OngoingBlockedTags = data.ongoingBlockedTags;
        }

        public AbilityTagContainer(
            GameplayTagSet assetTags,
            GameplayTagSet cancelAbilityTags,
            GameplayTagSet blockAbilityTags,
            GameplayTagSet activationOwnedTags,
            GameplayTagSet activationRequiredTags,
            GameplayTagSet activationBlockedTags,
            GameplayTagSet ongoingBlockedTags)
        {
            AssetTags = assetTags;
            CancelAbilitiesWithTags = cancelAbilityTags;
            BlockAbilitiesWithTags = blockAbilityTags;
            ActivationOwnedTags = activationOwnedTags;
            ActivationRequiredTags = activationRequiredTags;
            ActivationBlockedTags = activationBlockedTags;
            OngoingBlockedTags = ongoingBlockedTags;
        }
    }
}
