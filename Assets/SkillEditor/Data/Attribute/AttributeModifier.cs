using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 属性修改器 - 定义如何修改属性
    /// 对应虚幻引擎的 FGameplayModifierInfo
    /// </summary>
    [Serializable]
    public class AttributeModifier
    {
        /// <summary>
        /// 目标属性类型
        /// </summary>
        [SerializeField]
        private AttrType targetAttrType;
        public AttrType TargetAttrType
        {
            get => targetAttrType;
            set => targetAttrType = value;
        }

        /// <summary>
        /// 修改操作类型
        /// </summary>
        [SerializeField]
        private ModifierOperation operation = ModifierOperation.Add;
        public ModifierOperation Operation
        {
            get => operation;
            set => operation = value;
        }

        /// <summary>
        /// 基础数值
        /// </summary>
        [SerializeField]
        private float magnitude;
        public float Magnitude
        {
            get => magnitude;
            set => magnitude = value;
        }

        /// <summary>
        /// 数值公式（支持表达式，如 "Caster.Attack * 1.5"）
        /// 如果设置了公式，则优先使用公式计算
        /// </summary>
        [SerializeField]
        private string magnitudeFormula;
        public string MagnitudeFormula
        {
            get => magnitudeFormula;
            set => magnitudeFormula = value;
        }

        /// <summary>
        /// MMC 计算类型
        /// </summary>
        [SerializeField]
        private MMCType mmcType = MMCType.AttributeBased;
        public MMCType MMCType
        {
            get => mmcType;
            set => mmcType = value;
        }

        /// <summary>
        /// 是否使用 MMC 计算
        /// </summary>
        [SerializeField]
        private bool useMMC = false;
        public bool UseMMC
        {
            get => useMMC;
            set => useMMC = value;
        }

        /// <summary>
        /// 属性捕获类型
        /// </summary>
        [SerializeField]
        private AttributeCaptureType captureType = AttributeCaptureType.Snapshot;
        public AttributeCaptureType CaptureType
        {
            get => captureType;
            set => captureType = value;
        }

        // ============ MMC 详细配置 ============

        /// <summary>
        /// MMC 捕获的属性类型
        /// </summary>
        [SerializeField]
        private AttrType mmcCaptureAttribute = AttrType.Attack;
        public AttrType MMCCaptureAttribute
        {
            get => mmcCaptureAttribute;
            set => mmcCaptureAttribute = value;
        }

        /// <summary>
        /// MMC 属性来源
        /// </summary>
        [SerializeField]
        private MMCAttributeSource mmcAttributeSource = MMCAttributeSource.Source;
        public MMCAttributeSource MMCAttributeSource
        {
            get => mmcAttributeSource;
            set => mmcAttributeSource = value;
        }

        /// <summary>
        /// MMC 系数
        /// </summary>
        [SerializeField]
        private float mmcCoefficient = 1f;
        public float MMCCoefficient
        {
            get => mmcCoefficient;
            set => mmcCoefficient = value;
        }

        /// <summary>
        /// MMC 是否使用快照
        /// </summary>
        [SerializeField]
        private bool mmcUseSnapshot = true;
        public bool MMCUseSnapshot
        {
            get => mmcUseSnapshot;
            set => mmcUseSnapshot = value;
        }

        // ============ 构造函数 ============

        public AttributeModifier() { }

        public AttributeModifier(AttrType targetAttrType, ModifierOperation operation, float magnitude)
        {
            this.targetAttrType = targetAttrType;
            this.operation = operation;
            this.magnitude = magnitude;
        }

        // ============ 方法 ============

        /// <summary>
        /// 计算最终数值
        /// </summary>
        public float CalculateMagnitude(ModifierCalculationContext context)
        {
            // 优先使用 MMC 计算
            if (useMMC)
            {
                return CalculateMMC(context);
            }

            // 其次使用公式
            if (!string.IsNullOrEmpty(magnitudeFormula))
            {
                return EvaluateFormula(context);
            }

            // 最后使用基础数值
            return magnitude;
        }

        /// <summary>
        /// MMC 计算
        /// </summary>
        private float CalculateMMC(ModifierCalculationContext context)
        {
            if (mmcType == MMCType.AttributeBased)
            {
                // 基于属性的 MMC
                float? attrValue = null;

                if (mmcUseSnapshot && context?.SnapshotValues != null)
                {
                    // 使用快照值
                    attrValue = context.GetSnapshotValue(mmcCaptureAttribute);
                }

                if (!attrValue.HasValue)
                {
                    // 实时获取属性值
                    if (mmcAttributeSource == MMCAttributeSource.Source)
                    {
                        attrValue = context?.GetSourceAttribute(mmcCaptureAttribute);
                    }
                    else
                    {
                        attrValue = context?.GetTargetAttribute(mmcCaptureAttribute);
                    }
                }

                // 属性值 × 系数
                return (attrValue ?? 0f) * mmcCoefficient;
            }
            else if (mmcType == MMCType.LevelBased)
            {
                // 基于等级的 MMC
                int level = context?.EffectLevel ?? 1;
                return magnitude * (1 + level * 0.1f);
            }

            return magnitude;
        }

        /// <summary>
        /// 计算公式（简化实现，实际项目中可使用表达式解析器）
        /// </summary>
        private float EvaluateFormula(ModifierCalculationContext context)
        {
            // 简化实现：直接返回基础数值
            // 实际项目中应该实现完整的表达式解析
            // 例如解析 "Caster.Attack * 1.5 + 10"
            if (context == null) return magnitude;

            // TODO: 实现完整的公式解析
            // 这里可以集成第三方表达式解析库，如 NCalc
            return magnitude;
        }

        /// <summary>
        /// 从 AttributeModifierData 创建
        /// </summary>
        public static AttributeModifier FromData(AttributeModifierData data)
        {
            var modifier = new AttributeModifier
            {
                targetAttrType = data.attrType,
                operation = data.operation,
            };

            switch (data.magnitudeSourceType)
            {
                case ModifierMagnitudeSourceType.FixedValue:
                    modifier.magnitude = data.fixedValue;
                    break;
                case ModifierMagnitudeSourceType.Formula:
                    modifier.magnitudeFormula = data.formula;
                    break;
                case ModifierMagnitudeSourceType.ModifierMagnitudeCalculation:
                    modifier.useMMC = true;
                    modifier.mmcType = data.mmcType;
                    // 复制 MMC 详细配置
                    modifier.mmcCaptureAttribute = data.mmcCaptureAttribute;
                    modifier.mmcAttributeSource = data.mmcAttributeSource;
                    modifier.mmcCoefficient = data.mmcCoefficient;
                    modifier.mmcUseSnapshot = data.mmcUseSnapshot;
                    break;
                case ModifierMagnitudeSourceType.SetByCaller:
                    // SetByCaller 需要在运行时从上下文获取
                    break;
            }

            return modifier;
        }

        /// <summary>
        /// 转换为 AttributeModifierData
        /// </summary>
        public AttributeModifierData ToData()
        {
            var data = new AttributeModifierData
            {
                attrType = targetAttrType,
                operation = operation,
            };

            if (useMMC)
            {
                data.magnitudeSourceType = ModifierMagnitudeSourceType.ModifierMagnitudeCalculation;
                data.mmcType = mmcType;
            }
            else if (!string.IsNullOrEmpty(magnitudeFormula))
            {
                data.magnitudeSourceType = ModifierMagnitudeSourceType.Formula;
                data.formula = magnitudeFormula;
            }
            else
            {
                data.magnitudeSourceType = ModifierMagnitudeSourceType.FixedValue;
                data.fixedValue = magnitude;
            }

            return data;
        }

        public override string ToString()
        {
            string opStr = operation switch
            {
                ModifierOperation.Add => "+",
                ModifierOperation.Multiply => "*",
                ModifierOperation.Divide => "/",
                ModifierOperation.Override => "=",
                _ => "?"
            };

            string valueStr = !string.IsNullOrEmpty(magnitudeFormula) ? magnitudeFormula : magnitude.ToString();
            return $"{targetAttrType} {opStr} {valueStr}";
        }
    }

    /// <summary>
    /// 属性捕获类型 - 决定何时捕获属性值
    /// </summary>
    public enum AttributeCaptureType
    {
        /// <summary>
        /// 快照 - 在效果创建时捕获属性值
        /// </summary>
        Snapshot,

        /// <summary>
        /// 追踪 - 实时获取属性值
        /// </summary>
        Track
    }

    /// <summary>
    /// 修改器计算上下文 - 提供计算所需的信息
    /// </summary>
    public class ModifierCalculationContext
    {
        /// <summary>
        /// 效果来源的属性容器
        /// </summary>
        public AttributeSetContainer SourceAttributes { get; set; }

        /// <summary>
        /// 效果目标的属性容器
        /// </summary>
        public AttributeSetContainer TargetAttributes { get; set; }

        /// <summary>
        /// 快照属性值（效果创建时捕获）
        /// </summary>
        public System.Collections.Generic.Dictionary<AttrType, float> SnapshotValues { get; set; }

        /// <summary>
        /// 效果等级
        /// </summary>
        public int EffectLevel { get; set; } = 1;

        /// <summary>
        /// 自定义数据
        /// </summary>
        public System.Collections.Generic.Dictionary<string, object> CustomData { get; set; }

        /// <summary>
        /// 获取来源属性值
        /// </summary>
        public float? GetSourceAttribute(AttrType attrType)
        {
            return SourceAttributes?.GetCurrentValue(attrType);
        }

        /// <summary>
        /// 获取目标属性值
        /// </summary>
        public float? GetTargetAttribute(AttrType attrType)
        {
            return TargetAttributes?.GetCurrentValue(attrType);
        }

        /// <summary>
        /// 获取快照属性值
        /// </summary>
        public float? GetSnapshotValue(AttrType attrType)
        {
            if (SnapshotValues != null && SnapshotValues.TryGetValue(attrType, out float value))
            {
                return value;
            }
            return null;
        }

        /// <summary>
        /// 设置自定义数据
        /// </summary>
        public void SetCustomData(string key, object value)
        {
            CustomData ??= new System.Collections.Generic.Dictionary<string, object>();
            CustomData[key] = value;
        }

        /// <summary>
        /// 获取自定义数据
        /// </summary>
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (CustomData != null && CustomData.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }
    }
}
