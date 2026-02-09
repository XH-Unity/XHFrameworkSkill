using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 游戏属性 - 对应虚幻引擎的 FGameplayAttributeData
    /// 包含 BaseValue（永久值）和 CurrentValue（计算后的值）
    /// 内嵌 Aggregator 用于管理修改器
    /// </summary>
    [Serializable]
    public class Attribute
    {
        /// <summary>
        /// 属性类型
        /// </summary>
        [SerializeField]
        private AttrType attrType;
        public AttrType AttrType => attrType;

        /// <summary>
        /// 基础值 - 永久值，只被 Instant 效果修改
        /// </summary>
        [SerializeField]
        private float baseValue;
        public float BaseValue
        {
            get => baseValue;
            set
            {
                float oldValue = baseValue;
                float newValue = value;

                // 触发修改前回调
                OnPreBaseValueChange?.Invoke(this, ref newValue);

                // 应用限制
                if (hasMinValue) newValue = Mathf.Max(newValue, minValue);
                if (hasMaxValue) newValue = Mathf.Min(newValue, maxValue);

                baseValue = newValue;

                // BaseValue 变化后需要重新计算 CurrentValue
                MarkDirty();
                Recalculate();

                // 触发修改后回调
                OnPostBaseValueChange?.Invoke(this, oldValue, newValue);
            }
        }

        /// <summary>
        /// 当前值 - BaseValue + 所有活跃 Modifier 的结果
        /// </summary>
        [SerializeField]
        private float currentValue;
        public float CurrentValue
        {
            get => currentValue;
            set
            {
                float oldValue = currentValue;
                float newValue = value;

                // 触发修改前回调
                OnPreCurrentValueChange?.Invoke(this, ref newValue);

                // 应用限制
                if (hasMinValue) newValue = Mathf.Max(newValue, minValue);
                if (hasMaxValue) newValue = Mathf.Min(newValue, maxValue);

                currentValue = newValue;

                // 触发修改后回调
                OnPostCurrentValueChange?.Invoke(this, oldValue, newValue);
            }
        }

        /// <summary>
        /// 是否为 Meta 属性（临时占位符，如伤害值）
        /// </summary>
        [SerializeField]
        private bool isMeta;
        public bool IsMeta => isMeta;

        /// <summary>
        /// 是否有最小值限制
        /// </summary>
        [SerializeField]
        private bool hasMinValue;
        public bool HasMinValue => hasMinValue;

        /// <summary>
        /// 最小值
        /// </summary>
        [SerializeField]
        private float minValue;
        public float MinValue => minValue;

        /// <summary>
        /// 是否有最大值限制
        /// </summary>
        [SerializeField]
        private bool hasMaxValue;
        public bool HasMaxValue => hasMaxValue;

        /// <summary>
        /// 最大值
        /// </summary>
        [SerializeField]
        private float maxValue;
        public float MaxValue => maxValue;

        // ============ 内嵌 Aggregator ============

        /// <summary>
        /// 活跃的修改器列表
        /// </summary>
        [NonSerialized]
        private List<ActiveModifier> activeModifiers;

        /// <summary>
        /// 聚合模式
        /// </summary>
        [NonSerialized]
        private AggregatorMode aggregatorMode = AggregatorMode.Default;
        public AggregatorMode AggregatorMode
        {
            get => aggregatorMode;
            set => aggregatorMode = value;
        }

        /// <summary>
        /// 是否需要重新计算
        /// </summary>
        [NonSerialized]
        private bool isDirty = false;

        /// <summary>
        /// 活跃修改器数量
        /// </summary>
        public int ModifierCount => activeModifiers?.Count ?? 0;

        // ============ 事件回调 ============

        /// <summary>
        /// BaseValue 修改前回调 - 可用于 Clamp
        /// </summary>
        public delegate void PreValueChangeDelegate(Attribute attribute, ref float newValue);
        public event PreValueChangeDelegate OnPreBaseValueChange;
        public event PreValueChangeDelegate OnPreCurrentValueChange;

        /// <summary>
        /// 值修改后回调 - 用于 UI 更新等
        /// </summary>
        public delegate void PostValueChangeDelegate(Attribute attribute, float oldValue, float newValue);
        public event PostValueChangeDelegate OnPostBaseValueChange;
        public event PostValueChangeDelegate OnPostCurrentValueChange;

        // ============ 构造函数 ============

        public Attribute()
        {
            activeModifiers = new List<ActiveModifier>();
        }

        public Attribute(AttrType attrType, float defaultValue = 0f)
        {
            this.attrType = attrType;
            this.baseValue = defaultValue;
            this.currentValue = defaultValue;
            this.activeModifiers = new List<ActiveModifier>();
        }

        // ============ 基础方法 ============

        /// <summary>
        /// 初始化属性值（同时设置 BaseValue 和 CurrentValue）
        /// </summary>
        public void Initialize(float value)
        {
            baseValue = value;
            currentValue = value;
        }

        /// <summary>
        /// 设置值限制
        /// </summary>
        public void SetClamp(float? min, float? max)
        {
            hasMinValue = min.HasValue;
            if (min.HasValue) minValue = min.Value;

            hasMaxValue = max.HasValue;
            if (max.HasValue) maxValue = max.Value;
        }

        /// <summary>
        /// 设置为 Meta 属性
        /// </summary>
        public void SetAsMeta(bool isMeta = true)
        {
            this.isMeta = isMeta;
        }

        /// <summary>
        /// 重置当前值为基础值
        /// </summary>
        public void ResetCurrentToBase()
        {
            CurrentValue = BaseValue;
        }

        /// <summary>
        /// 清除所有事件监听
        /// </summary>
        public void ClearCallbacks()
        {
            OnPreBaseValueChange = null;
            OnPreCurrentValueChange = null;
            OnPostBaseValueChange = null;
            OnPostCurrentValueChange = null;
        }

        // ============ Aggregator 方法 ============

        /// <summary>
        /// 添加修改器
        /// </summary>
        public void AddModifier(AttributeModifier modifier, object source = null)
        {
            if (activeModifiers == null)
                activeModifiers = new List<ActiveModifier>();

            var activeModifier = new ActiveModifier
            {
                Modifier = modifier,
                Source = source,
                AppliedTime = Time.time
            };

            activeModifiers.Add(activeModifier);
            MarkDirty();
        }

        /// <summary>
        /// 移除修改器
        /// </summary>
        public bool RemoveModifier(AttributeModifier modifier)
        {
            if (activeModifiers == null) return false;

            int removed = activeModifiers.RemoveAll(m => m.Modifier == modifier);
            if (removed > 0)
            {
                MarkDirty();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 移除来自指定源的所有修改器
        /// </summary>
        public int RemoveModifiersFromSource(object source)
        {
            if (activeModifiers == null) return 0;

            int removed = activeModifiers.RemoveAll(m => m.Source == source);
            if (removed > 0)
            {
                MarkDirty();
            }
            return removed;
        }

        /// <summary>
        /// 清除所有修改器
        /// </summary>
        public void ClearModifiers()
        {
            activeModifiers?.Clear();
            MarkDirty();
        }

        /// <summary>
        /// 标记为需要重新计算
        /// </summary>
        public void MarkDirty()
        {
            isDirty = true;
        }

        /// <summary>
        /// 重新计算 CurrentValue
        /// </summary>
        public void Recalculate(ModifierCalculationContext context = null)
        {
            if (!isDirty) return;

            float newValue = CalculateNewValue(context);
            CurrentValue = newValue;
            isDirty = false;
        }

        /// <summary>
        /// 计算新值
        /// </summary>
        private float CalculateNewValue(ModifierCalculationContext context)
        {
            if (activeModifiers == null || activeModifiers.Count == 0)
                return baseValue;

            // 根据聚合模式筛选修改器
            var filteredModifiers = FilterModifiers(activeModifiers);

            // 分组计算：先加法，再乘法，最后覆盖
            float additive = 0f;
            float multiplicative = 1f;
            float? overrideValue = null;

            foreach (var activeModifier in filteredModifiers)
            {
                var modifier = activeModifier.Modifier;
                float magnitude = modifier.CalculateMagnitude(context);

                // 从来源获取堆叠层数（通过反射获取 StackCount 属性）
                int stackCount = 1;
                var source = activeModifier.Source;
                if (source != null)
                {
                    var stackCountProp = source.GetType().GetProperty("StackCount");
                    if (stackCountProp != null)
                    {
                        stackCount = (int)stackCountProp.GetValue(source);
                    }
                }

                switch (modifier.Operation)
                {
                    case ModifierOperation.Add:
                        // 加法：直接乘以层数（10伤害 × 3层 = 30伤害）
                        additive += magnitude * stackCount;
                        break;
                    case ModifierOperation.Multiply:
                        // 乘法：转换为百分比累加（1.1 表示 +10%，3层 = +30% = 1.3）
                        // 公式：1 + (magnitude - 1) * stackCount
                        float percentBonus = (magnitude - 1f) * stackCount;
                        multiplicative *= (1f + percentBonus);
                        break;
                    case ModifierOperation.Divide:
                        // 除法：同样转换为百分比
                        if (Mathf.Abs(magnitude) > 0.0001f)
                        {
                            float divideBonus = (magnitude - 1f) * stackCount;
                            multiplicative /= (1f + divideBonus);
                        }
                        break;
                    case ModifierOperation.Override:
                        overrideValue = magnitude;
                        break;
                }
            }

            // 计算最终值
            // 公式: (BaseValue + Additive) * Multiplicative
            // 如果有 Override，则直接使用 Override 值
            if (overrideValue.HasValue)
            {
                return overrideValue.Value;
            }

            return (baseValue + additive) * multiplicative;
        }

        /// <summary>
        /// 根据聚合模式筛选修改器
        /// </summary>
        private List<ActiveModifier> FilterModifiers(List<ActiveModifier> modifiers)
        {
            switch (aggregatorMode)
            {
                case AggregatorMode.MostNegativeModifier:
                    return FilterMostNegative(modifiers);

                case AggregatorMode.MostPositiveModifier:
                    return FilterMostPositive(modifiers);

                case AggregatorMode.MostNegativeWithAllPositive:
                    return FilterMostNegativeWithAllPositive(modifiers);

                case AggregatorMode.Default:
                default:
                    return modifiers;
            }
        }

        private List<ActiveModifier> FilterMostNegative(List<ActiveModifier> modifiers)
        {
            var result = new List<ActiveModifier>();
            ActiveModifier? mostNegative = null;
            float mostNegativeValue = 0f;

            foreach (var mod in modifiers)
            {
                if (mod.Modifier.Operation == ModifierOperation.Add)
                {
                    float magnitude = mod.Modifier.CalculateMagnitude(null);
                    if (magnitude < 0 && magnitude < mostNegativeValue)
                    {
                        mostNegative = mod;
                        mostNegativeValue = magnitude;
                    }
                }
                else
                {
                    result.Add(mod);
                }
            }

            if (mostNegative.HasValue)
            {
                result.Add(mostNegative.Value);
            }

            return result;
        }

        private List<ActiveModifier> FilterMostPositive(List<ActiveModifier> modifiers)
        {
            var result = new List<ActiveModifier>();
            ActiveModifier? mostPositive = null;
            float mostPositiveValue = 0f;

            foreach (var mod in modifiers)
            {
                if (mod.Modifier.Operation == ModifierOperation.Add)
                {
                    float magnitude = mod.Modifier.CalculateMagnitude(null);
                    if (magnitude > 0 && magnitude > mostPositiveValue)
                    {
                        mostPositive = mod;
                        mostPositiveValue = magnitude;
                    }
                }
                else
                {
                    result.Add(mod);
                }
            }

            if (mostPositive.HasValue)
            {
                result.Add(mostPositive.Value);
            }

            return result;
        }

        private List<ActiveModifier> FilterMostNegativeWithAllPositive(List<ActiveModifier> modifiers)
        {
            var result = new List<ActiveModifier>();
            ActiveModifier? mostNegative = null;
            float mostNegativeValue = 0f;

            foreach (var mod in modifiers)
            {
                if (mod.Modifier.Operation == ModifierOperation.Add)
                {
                    float magnitude = mod.Modifier.CalculateMagnitude(null);
                    if (magnitude >= 0)
                    {
                        result.Add(mod);
                    }
                    else if (magnitude < mostNegativeValue)
                    {
                        mostNegative = mod;
                        mostNegativeValue = magnitude;
                    }
                }
                else
                {
                    result.Add(mod);
                }
            }

            if (mostNegative.HasValue)
            {
                result.Add(mostNegative.Value);
            }

            return result;
        }

        /// <summary>
        /// 获取所有活跃修改器
        /// </summary>
        public IEnumerable<ActiveModifier> GetActiveModifiers()
        {
            return activeModifiers ?? (IEnumerable<ActiveModifier>)Array.Empty<ActiveModifier>();
        }

        public override string ToString()
        {
            return $"{attrType}: Base={BaseValue}, Current={CurrentValue}";
        }
    }

    /// <summary>
    /// 活跃修改器 - 包含修改器和来源信息
    /// </summary>
    public struct ActiveModifier
    {
        /// <summary>
        /// 修改器
        /// </summary>
        public AttributeModifier Modifier;

        /// <summary>
        /// 来源（如 GameplayEffectSpec）
        /// </summary>
        public object Source;

        /// <summary>
        /// 应用时间
        /// </summary>
        public float AppliedTime;
    }

    /// <summary>
    /// 聚合模式 - 对应虚幻引擎的 FAggregatorEvaluateMetaData
    /// </summary>
    public enum AggregatorMode
    {
        /// <summary>
        /// 默认 - 所有修改器都参与计算
        /// </summary>
        Default,

        /// <summary>
        /// 只保留最负的加法修改器
        /// </summary>
        MostNegativeModifier,

        /// <summary>
        /// 只保留最正的加法修改器
        /// </summary>
        MostPositiveModifier,

        /// <summary>
        /// 保留最负的加法修改器和所有正加法修改器
        /// 用于移动速度等场景（Paragon 模式）
        /// </summary>
        MostNegativeWithAllPositive
    }
}
