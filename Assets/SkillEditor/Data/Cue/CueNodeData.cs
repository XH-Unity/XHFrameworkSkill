using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 表现节点数据基类 - 对应 GAS 的 GameplayCue
    /// 用于播放视觉/音效表现，不改变游戏状态
    /// </summary>
    [Serializable]
    public class CueNodeData : NodeData
    {
        /// <summary>
        /// 播放所需标签 - 目标必须拥有这些标签才能播放表现
        /// </summary>
        public GameplayTagSet requiredTags;

        /// <summary>
        /// 播放阻止标签 - 目标拥有这些标签时阻止播放表现
        /// </summary>
        public GameplayTagSet immunityTags;
        /// <summary>
        /// 随节点销毁（当触发节点结束时销毁特效）
        /// </summary>
        public bool destroyWithNode = false;
    }

    public struct CueTagContainer
    {
        public GameplayTagSet RequiredTags;
        public GameplayTagSet ImmunityTags;

        public CueTagContainer(CueNodeData data)
        {
            RequiredTags = data.requiredTags;
            ImmunityTags = data.immunityTags;
        }

        public CueTagContainer(
            GameplayTagSet requiredTags,
            GameplayTagSet immunityTags
         )
        {
            RequiredTags = requiredTags;
            ImmunityTags = immunityTags;
        }
    }
}
