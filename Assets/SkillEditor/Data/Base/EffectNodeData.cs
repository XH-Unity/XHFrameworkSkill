using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 效果节点数据基类 - 所有效果节点的公共数据
    /// 重构后的统一基类，包含所有Effect通用属性
    /// </summary>
    [Serializable]
    public abstract class EffectNodeData : NodeData
    {
        // ============ GAS 标签系统 ============

        /// <summary>
        /// 效果自身标签 - 用于标识此效果
        /// </summary>
        public GameplayTagSet assetTags;

        /// <summary>
        /// 激活时授予标签 - 效果激活时给目标添加的标签
        /// </summary>
        public GameplayTagSet grantedTags;

        /// <summary>
        /// 应用所需标签 - 目标必须拥有这些标签才能应用效果
        /// </summary>
        public GameplayTagSet applicationRequiredTags;

        /// <summary>
        /// 应用阻止标签 - 目标拥有这些标签时阻止应用效果
        /// </summary>
        public GameplayTagSet applicationImmunityTags;

        /// <summary>
        /// 移除带有标签的效果 - 应用此效果时移除目标身上带有这些标签的其他效果
        /// </summary>
        public GameplayTagSet removeGameEffectsWithTags;

        // ============ 持续类型配置 ============

        /// <summary>
        /// 效果持续类型（瞬时/持续/永久）
        /// </summary>
        public EffectDurationType durationType = EffectDurationType.Instant;

        /// <summary>
        /// 持续时间（支持公式，如 "5" 或 "StackCount * 2"）
        /// </summary>
        public string duration = "0";

        // ============ 周期配置 ============

        /// <summary>
        /// 是否为周期效果
        /// </summary>
        public bool isPeriodic = false;

        /// <summary>
        /// 执行周期（支持公式，如 "1" 或 "BasePeriod * 0.5"）
        /// </summary>
        public string period = "1";

        /// <summary>
        /// 应用时立即执行一次
        /// </summary>
        public bool executeOnApplication = false;

        // ============ 属性修改器 ============

        /// <summary>
        /// 属性修改器列表
        /// </summary>
        public List<AttributeModifierData> attributeModifiers = new List<AttributeModifierData>();

        // ============ 堆叠配置 ============

        /// <summary>
        /// 堆叠类型
        /// </summary>
        public StackType stackType = StackType.None;

        /// <summary>
        /// 堆叠上限（0=无限）
        /// </summary>
        public int stackLimit = 0;

        /// <summary>
        /// 堆叠时持续时间刷新策略
        /// </summary>
        public StackDurationRefreshPolicy stackDurationRefreshPolicy = StackDurationRefreshPolicy.RefreshOnSuccessfulApplication;

        /// <summary>
        /// 堆叠时周期重置策略
        /// </summary>
        public StackPeriodResetPolicy stackPeriodResetPolicy = StackPeriodResetPolicy.ResetOnSuccessfulApplication;

        /// <summary>
        /// 堆叠过期策略 - 决定Effect过期时如何处理堆叠
        /// </summary>
        public StackExpirationPolicy stackExpirationPolicy = StackExpirationPolicy.ClearEntireStack;

        /// <summary>
        /// 堆叠溢出策略 - 当堆叠达到上限时的处理
        /// </summary>
        public StackOverflowPolicy stackOverflowPolicy = StackOverflowPolicy.DenyApplication;
        
        // ============ 持续所需标签 ============

        /// <summary>
        /// 持续所需标签 - 效果持续期间目标必须拥有的标签（不满足则移除）
        /// </summary>
        public GameplayTagSet ongoingRequiredTags;

        // ============ 移除策略 ============

        /// <summary>
        /// 是否随技能结束而取消
        /// true: 技能结束时取消此 Effect
        /// false: 不随技能结束移除，由 ASC 管理生命周期（如 CD、Buff）
        /// </summary>
        public bool cancelOnAbilityEnd = false;
    }

    /// <summary>
    /// 效果标签容器（运行时使用）
    /// </summary>
    public struct EffectTagContainer
    {
        public GameplayTagSet AssetTags;
        public GameplayTagSet GrantedTags;
        public GameplayTagSet ApplicationRequiredTags;
        public GameplayTagSet OngoingRequiredTags;
        public GameplayTagSet RemoveGameplayEffectsWithTags;
        public GameplayTagSet ApplicationImmunityTags;

        public EffectTagContainer(EffectNodeData data)
        {
            AssetTags = data.assetTags;
            GrantedTags = data.grantedTags;
            ApplicationRequiredTags = data.applicationRequiredTags;
            RemoveGameplayEffectsWithTags = data.removeGameEffectsWithTags;
            ApplicationImmunityTags = data.applicationImmunityTags;
            OngoingRequiredTags = data.ongoingRequiredTags;
        }

        public EffectTagContainer(
            GameplayTagSet assetTags,
            GameplayTagSet grantedTags,
            GameplayTagSet applicationRequiredTags,
            GameplayTagSet ongoingRequiredTags,
            GameplayTagSet removeGameplayEffectsWithTags,
            GameplayTagSet applicationImmunityTags)
        {
            AssetTags = assetTags;
            GrantedTags = grantedTags;
            ApplicationRequiredTags = applicationRequiredTags;
            OngoingRequiredTags = ongoingRequiredTags;
            RemoveGameplayEffectsWithTags = removeGameplayEffectsWithTags;
            ApplicationImmunityTags = applicationImmunityTags;
        }
    }
}
