using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 节点类型枚举 - 按GAS概念分类
    /// 【重要】每个枚举值必须显式指定数值，防止调整顺序或插入新值时导致序列化数据错乱
    /// 新增枚举时，请使用未被占用的数值（建议从 20 开始），不要修改已有枚举的值
    /// </summary>
    public enum NodeType
    {
        // ============ 技能节点 ============
        [InspectorName("技能")]
        Ability = 0,

        // ============ 效果节点 (Effect) - 对应 GameplayEffect ============

        // --- 瞬时效果 (Instant) ---
        [InspectorName("伤害效果")]
        DamageEffect = 1,
        [InspectorName("治疗效果")]
        HealEffect = 2,
        [InspectorName("消耗效果")]
        CostEffect = 3,
        [InspectorName("属性修改效果")]
        ModifyAttributeEffect = 6,

        // --- 投射物效果 ---
        [InspectorName("投射物效果")]
        ProjectileEffect = 8,

        // --- 放置物效果 ---
        [InspectorName("放置物效果")]
        PlacementEffect = 9,

        // --- 持续效果 (Duration) ---
        [InspectorName("冷却效果")]
        CooldownEffect = 10,
        [InspectorName("Buff效果")]
        BuffEffect = 11,

        // ============ 表现节点 (Cue) - 对应 GameplayCue ============
        [InspectorName("粒子特效")]
        ParticleCue = 12,
        [InspectorName("音效")]
        SoundCue = 13,
        [InspectorName("飘字")]
        FloatingTextCue = 14,

        // ============ 任务节点 (Task) - 执行特定任务 ============
        [InspectorName("搜索目标")]
        SearchTargetTask = 4,      // 保持原值，避免数据迁移
        [InspectorName("结束技能")]
        EndAbilityTask = 7,        // 保持原值

        // ============ 条件节点 (Condition) - 条件判断分支 ============
        [InspectorName("属性比较")]
        AttributeCompareCondition = 5,  // 保持原值

        // ============ 新增节点类型请从这里开始（20+）============
        // 示例：
        // [InspectorName("新效果")]
        // NewEffect = 20,

        [InspectorName("动画")]
        Animation = 20,

        [InspectorName("通用效果")]
        GenericEffect = 21,

        [InspectorName("位移效果")]
        DisplaceEffect = 22,
    }

    /// <summary>
    /// 节点分类 - 包含5种：Root/Effect/Cue/Task/Condition
    /// </summary>
    public enum NodeCategory
    {
        [InspectorName("根节点")]
        Root,
        [InspectorName("效果节点")]
        Effect,
        [InspectorName("表现节点")]
        Cue,
        [InspectorName("任务节点")]
        Task,
        [InspectorName("条件节点")]
        Condition
    }

    /// <summary>
    /// 效果持续类型 - 对应GAS的GameplayEffectDurationType
    /// </summary>
    public enum EffectDurationType
    {
        [InspectorName("瞬时")]
        Instant,
        [InspectorName("持续")]
        Duration,
        [InspectorName("永久")]
        Infinite
    }

    /// <summary>
    /// 节点目标枚举 - 用于确定效果作用的目标（ASC）
    /// </summary>
    public enum TargetType
    {
        [InspectorName("父节点传入")]
        ParentInput,
        [InspectorName("施法者")]
        Caster,
        [InspectorName("技能主目标")]
        MainTarget
    }

    /// <summary>
    /// 位移类型 - 用于位移效果节点
    /// </summary>
    public enum DisplaceType
    {
        [InspectorName("吸引到施法者")]
        Pull,
        [InspectorName("从施法者击退")]
        Push,
        [InspectorName("吸引到指定点")]
        PullToPoint
    }

    /// <summary>
    /// 位置来源类型 - 用于确定位置信息的获取方式
    /// </summary>
    public enum PositionSourceType
    {
        [InspectorName("施法者")]
        Caster,
        [InspectorName("技能主目标")]
        MainTarget,
        [InspectorName("父节点传入")]
        ParentInput,
        [InspectorName("投射物")]
        Projectile,
        [InspectorName("放置物")]
        Placement,
      
    }

    /// <summary>
    /// 搜索形状枚举
    /// </summary>
    public enum SearchShapeType
    {
        [InspectorName("圆形")]
        Circle,
        [InspectorName("扇形")]
        Sector,
        [InspectorName("直线")]
        Line
    }

    /// <summary>
    /// 直线类型枚举
    /// </summary>
    public enum LineType
    {
        [InspectorName("单位朝向")]
        UnitDirection,
        [InspectorName("两点之间")]
        BetweenPoints,
        [InspectorName("绝对角度值")]
        AbsoluteAngle
    }

    /// <summary>
    /// 属性修改器操作类型 - 对应GAS的GameplayModifierOp
    /// </summary>
    public enum ModifierOperation
    {
        [InspectorName("加法")]
        Add,
        [InspectorName("乘法")]
        Multiply,
        [InspectorName("除法")]
        Divide,
        [InspectorName("覆盖")]
        Override
    }

    /// <summary>
    /// 堆叠类型 - 对应GAS的GameplayEffectStackingType
    /// </summary>
    public enum StackType
    {
        [InspectorName("不堆叠")]
        None,
        [InspectorName("按目标堆叠")]
        AggregateByTarget,
        [InspectorName("按源堆叠")]
        AggregateBySource
    }

    /// <summary>
    /// 堆叠时持续时间刷新策略 - 对应GAS的EGameplayEffectStackingDurationPolicy
    /// </summary>
    public enum StackDurationRefreshPolicy
    {
        [InspectorName("刷新时间")]
        RefreshOnSuccessfulApplication,
        [InspectorName("不刷新")]
        NeverRefresh
    }

    /// <summary>
    /// 堆叠时周期重置策略 - 对应GAS的EGameplayEffectStackingPeriodPolicy
    /// </summary>
    public enum StackPeriodResetPolicy
    {
        [InspectorName("重置周期")]
        ResetOnSuccessfulApplication,
        [InspectorName("不重置")]
        NeverReset
    }

    /// <summary>
    /// 堆叠过期策略 - 对应GAS的EGameplayEffectStackingExpirationPolicy
    /// 决定Effect过期时如何处理堆叠
    /// </summary>
    public enum StackExpirationPolicy
    {
        [InspectorName("清除全部堆叠")]
        ClearEntireStack,
        [InspectorName("移除一层并刷新时间")]
        RemoveSingleStackAndRefreshDuration,
        [InspectorName("只刷新时间")]
        RefreshDuration
    }

    /// <summary>
    /// 堆叠溢出策略 - 当堆叠达到上限时的处理
    /// </summary>
    public enum StackOverflowPolicy
    {
        [InspectorName("拒绝新应用")]
        DenyApplication,
        [InspectorName("允许溢出效果")]
        AllowOverflowEffect
    }

    /// <summary>
    /// 事件类型枚举 - 用于Buff事件监听
    /// </summary>
    public enum GameplayEventType
    {
        [InspectorName("受击")]
        OnHit,
        [InspectorName("造成伤害")]
        OnDealDamage,
        [InspectorName("受到伤害")]
        OnTakeDamage,
        [InspectorName("死亡")]
        OnDeath,
        [InspectorName("击杀")]
        OnKill,
        [InspectorName("自定义事件")]
        Custom
    }

    /// <summary>
    /// 伤害类型枚举
    /// </summary>
    public enum DamageType
    {
        [InspectorName("物理伤害")]
        Physical,
        [InspectorName("魔法伤害")]
        Magical,
        [InspectorName("真实伤害")]
        True
    }

    /// <summary>
    /// 技能结束类型 - 对应GAS的EndAbility方式
    /// </summary>
    public enum EndAbilityType
    {
        [InspectorName("正常结束")]
        Normal,
        [InspectorName("取消")]
        Cancel
    }

    /// <summary>
    /// 标签检查模式
    /// </summary>
    public enum TagCheckMode
    {
        [InspectorName("拥有标签")]
        HasTag,
        [InspectorName("不拥有标签")]
        NotHasTag,
        [InspectorName("拥有全部标签")]
        HasAllTags,
        [InspectorName("拥有任意标签")]
        HasAnyTag
    }

    /// <summary>
    /// 比较操作符
    /// </summary>
    public enum CompareOperator
    {
        [InspectorName("等于")]
        Equal,
        [InspectorName("不等于")]
        NotEqual,
        [InspectorName("大于")]
        Greater,
        [InspectorName("大于等于")]
        GreaterOrEqual,
        [InspectorName("小于")]
        Less,
        [InspectorName("小于等于")]
        LessOrEqual
    }

    /// <summary>
    /// 属性比较值类型
    /// </summary>
    public enum AttributeValueType
    {
        [InspectorName("固定值")]
        Fixed,
        [InspectorName("百分比")]
        Percentage
    }

    /// <summary>
    /// 修改器数值来源类型 - 决定 Modifier 的数值如何计算
    /// </summary>
    public enum ModifierMagnitudeSourceType
    {
        [InspectorName("具体值")]
        FixedValue,

        [InspectorName("公式")]
        Formula,

        [InspectorName("自定义计算器 (MMC)")]
        ModifierMagnitudeCalculation,

        [InspectorName("上下文数据")]
        SetByCaller
    }

    /// <summary>
    /// 伤害计算类型枚举 - 选择使用哪个伤害计算类
    /// </summary>
    public enum DamageCalculationType
    {
        [InspectorName("默认伤害计算")]
        Default
        // 扩展时在这里添加新的计算类型
        // [InspectorName("百分比伤害")]
        // PercentageDamage,
    }

    /// <summary>
    /// 治疗计算类型枚举 - 选择使用哪个治疗计算类
    /// </summary>
    public enum HealCalculationType
    {
        [InspectorName("默认治疗计算")]
        Default
        // 扩展时在这里添加新的计算类型
        // [InspectorName("百分比治疗")]
        // PercentageHeal,
    }

    /// <summary>
    /// MMC 计算类型枚举 - 选择使用哪个 MMC 计算类
    /// </summary>
    public enum MMCType
    {
        [InspectorName("基于属性")]
        AttributeBased,
        [InspectorName("基于等级")]
        LevelBased
        // 扩展时在这里添加新的 MMC 类型
    }
}
