using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 执行计算上下文 - 传递给计算类的数据
    /// </summary>
    public struct ExecutionContext
    {
        /// <summary>
        /// 施法者/来源
        /// </summary>
        public object Caster;

        /// <summary>
        /// 目标/应用者
        /// </summary>
        public object Target;

        /// <summary>
        /// 基础数值（从编辑器配置的数值）
        /// </summary>
        public float BaseMagnitude;

        /// <summary>
        /// 伤害类型（仅伤害计算使用）
        /// </summary>
        public DamageType DamageType;

        /// <summary>
        /// 技能等级
        /// </summary>
        public int SkillLevel;

        /// <summary>
        /// 上下文数据（SetByCaller 数据）
        /// </summary>
        public Dictionary<string, float> ContextData;
    }

    /// <summary>
    /// 执行计算结果
    /// </summary>
    public struct ExecutionResult
    {
        /// <summary>
        /// 最终计算值
        /// </summary>
        public float FinalValue;

        /// <summary>
        /// 是否暴击
        /// </summary>
        public bool IsCritical;

        /// <summary>
        /// 额外信息（可选）
        /// </summary>
        public Dictionary<string, object> ExtraData;
    }

    #region 伤害计算

    /// <summary>
    /// 伤害计算基类
    /// </summary>
    public abstract class DamageExecutionCalculation
    {
        /// <summary>
        /// 执行伤害计算
        /// </summary>
        public abstract ExecutionResult Execute(ExecutionContext context);

        /// <summary>
        /// 根据枚举获取对应的计算实例
        /// </summary>
        public static DamageExecutionCalculation Get(DamageCalculationType type)
        {
            return type switch
            {
                DamageCalculationType.Default => DefaultDamageCalculation.Instance,
                _ => DefaultDamageCalculation.Instance
            };
        }
    }

    /// <summary>
    /// 默认伤害计算 - 基础伤害公式
    /// 公式: (基础伤害 - 目标防御 * 防御系数) * 伤害类型系数
    /// </summary>
    public class DefaultDamageCalculation : DamageExecutionCalculation
    {
        public static readonly DefaultDamageCalculation Instance = new();

        public override ExecutionResult Execute(ExecutionContext context)
        {
            float baseDamage = context.BaseMagnitude;

            // TODO: 从 Caster 和 Target 获取属性进行计算
            // 这里写死计算逻辑，实际项目中根据需求修改

            // 示例计算逻辑：
            // float attack = GetAttribute(context.Caster, "Fight.Attack");
            // float defense = GetAttribute(context.Target, "Fight.Defense");
            // float finalDamage = baseDamage + attack - defense * 0.5f;

            float finalDamage = baseDamage;

            // 伤害类型系数
            float typeMultiplier = context.DamageType switch
            {
                DamageType.Physical => 1.0f,
                DamageType.Magical => 1.0f,
                DamageType.True => 1.0f,  // 真实伤害无视防御
                _ => 1.0f
            };

            finalDamage *= typeMultiplier;

            // 确保伤害不为负
            finalDamage = Mathf.Max(0, finalDamage);

            return new ExecutionResult
            {
                FinalValue = finalDamage,
                IsCritical = false
            };
        }
    }

    #endregion

    #region 治疗计算

    /// <summary>
    /// 治疗计算基类
    /// </summary>
    public abstract class HealExecutionCalculation
    {
        /// <summary>
        /// 执行治疗计算
        /// </summary>
        public abstract ExecutionResult Execute(ExecutionContext context);

        /// <summary>
        /// 根据枚举获取对应的计算实例
        /// </summary>
        public static HealExecutionCalculation Get(HealCalculationType type)
        {
            return type switch
            {
                HealCalculationType.Default => DefaultHealCalculation.Instance,
                _ => DefaultHealCalculation.Instance
            };
        }
    }

    /// <summary>
    /// 默认治疗计算
    /// </summary>
    public class DefaultHealCalculation : HealExecutionCalculation
    {
        public static readonly DefaultHealCalculation Instance = new();

        public override ExecutionResult Execute(ExecutionContext context)
        {
            float baseHeal = context.BaseMagnitude;

            // TODO: 从 Caster 获取属性进行计算
            // 示例：
            // float healPower = GetAttribute(context.Caster, "Fight.HealPower");
            // float finalHeal = baseHeal * (1 + healPower * 0.01f);

            float finalHeal = baseHeal;

            // 确保治疗不为负
            finalHeal = Mathf.Max(0, finalHeal);

            return new ExecutionResult
            {
                FinalValue = finalHeal,
                IsCritical = false
            };
        }
    }

    #endregion

    #region MMC 计算

    /// <summary>
    /// MMC 计算基类 - 用于属性修改器的数值计算
    /// </summary>
    public abstract class MMCCalculation
    {
        /// <summary>
        /// 计算数值
        /// </summary>
        public abstract float Calculate(ExecutionContext context);

        /// <summary>
        /// 根据枚举获取对应的计算实例
        /// </summary>
        public static MMCCalculation Get(MMCType type)
        {
            return type switch
            {
                MMCType.AttributeBased => AttributeBasedMMC.Instance,
                MMCType.LevelBased => LevelBasedMMC.Instance,
                _ => AttributeBasedMMC.Instance
            };
        }
    }

    /// <summary>
    /// 基于属性的 MMC - 根据某个属性计算数值
    /// </summary>
    public class AttributeBasedMMC : MMCCalculation
    {
        public static readonly AttributeBasedMMC Instance = new();

        public override float Calculate(ExecutionContext context)
        {
            // TODO: 实现基于属性的计算
            // 示例: return GetAttribute(context.Caster, "Fight.Strength") * 2;
            return context.BaseMagnitude;
        }
    }

    /// <summary>
    /// 基于等级的 MMC - 根据技能等级计算数值
    /// </summary>
    public class LevelBasedMMC : MMCCalculation
    {
        public static readonly LevelBasedMMC Instance = new();

        public override float Calculate(ExecutionContext context)
        {
            // 示例: 每级增加 10%
            return context.BaseMagnitude * (1 + context.SkillLevel * 0.1f);
        }
    }

    #endregion
}
