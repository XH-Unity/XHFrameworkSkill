using SkillEditor.Data;
using SkillEditor.Runtime.Utils;

namespace SkillEditor.Runtime
{
    /// <summary>
    /// 伤害效果Spec（瞬时效果）
    /// </summary>
    public class DamageEffectSpec : GameplayEffectSpec
    {
        private DamageEffectNodeData DamageNodeData => NodeData as DamageEffectNodeData;

        protected override void OnInitialHook(AbilitySystemComponent target)
        {
            if (target == null) return;
            var nodeData = DamageNodeData;
            if (nodeData == null) return;

            // 计算伤害
            float baseDamage = CalculateDamage(nodeData, target);

            // 如果勾选了"乘以堆叠层数"，从上下文获取层数并相乘
            if (nodeData.damageMultiplyByStackCount)
            {
                int stackCount = Context?.StackCount ?? 1;
                baseDamage *= stackCount;
            }

            // 应用护甲减伤（真实伤害不减免）
            if (nodeData.damageCalculationType == DamageCalculationType.Default
                && nodeData.damageType != DamageType.True
                && target.Attributes != null)
            {
                var defenseType = nodeData.damageType == DamageType.Physical ? AttrType.Defense : AttrType.MagicDefense;
                float? defense = target.Attributes.GetCurrentValue(defenseType);
                if (defense.HasValue && defense.Value > 0)
                    baseDamage *= 100f / (100f + defense.Value);
            }

            baseDamage = UnityEngine.Mathf.Max(0f, baseDamage);

            // 应用伤害
            if (target.Attributes != null && baseDamage > 0)
            {
                var healthAttr = target.Attributes.GetAttribute(AttrType.Health);
                if (healthAttr != null)
                {
                    healthAttr.BaseValue -= baseDamage;
                }

                // 将伤害结果存入上下文，供飘字Cue使用
                var ctx = GetExecutionContext();
                var damageResult = new Data.DamageResult(baseDamage, false, false, nodeData.damageType);
                ctx.SetCustomData("DamageResult", damageResult);
            }
        }

        /// <summary>
        /// 计算伤害值
        /// </summary>
        private float CalculateDamage(DamageEffectNodeData nodeData, AbilitySystemComponent target)
        {
            switch (nodeData.damageSourceType)
            {
                case ModifierMagnitudeSourceType.FixedValue:
                    return nodeData.damageFixedValue;

                case ModifierMagnitudeSourceType.Formula:
                    return FormulaEvaluator.Evaluate(nodeData.damageFormula, new FormulaContext
                    {
                        CasterAttributes = Context?.Caster?.Attributes,
                        TargetAttributes = target.Attributes,
                        Level = Level,
                        StackCount = Context?.StackCount ?? 1
                    });

                case ModifierMagnitudeSourceType.SetByCaller:
                    return GetSetByCallerValue(nodeData.damageSetByCallerKey, 0f);

                case ModifierMagnitudeSourceType.ModifierMagnitudeCalculation:
                    return CalculateMMCDamage(nodeData, target);

                default:
                    return 0f;
            }
        }

        /// <summary>
        /// 使用 MMC 计算伤害
        /// </summary>
        private float CalculateMMCDamage(DamageEffectNodeData nodeData, AbilitySystemComponent target)
        {
            if (nodeData.damageMMCType == MMCType.AttributeBased)
            {
                // 基于属性的 MMC
                float? attrValue = null;

                // 根据快照设置决定使用快照值还是实时值
                if (nodeData.damageMMCUseSnapshot && SnapshotValues != null)
                {
                    if (SnapshotValues.TryGetValue(nodeData.damageMMCCaptureAttribute, out float snapshotValue))
                    {
                        attrValue = snapshotValue;
                    }
                }

                // 如果没有快照值，实时获取
                if (!attrValue.HasValue)
                {
                    if (nodeData.damageMMCAttributeSource == MMCAttributeSource.Source)
                    {
                        // 从施法者获取属性
                        attrValue = Source?.Attributes?.GetCurrentValue(nodeData.damageMMCCaptureAttribute);
                    }
                    else
                    {
                        // 从目标获取属性
                        attrValue = target?.Attributes?.GetCurrentValue(nodeData.damageMMCCaptureAttribute);
                    }
                }

                // 属性值 × 系数
                return (attrValue ?? 0f) * nodeData.damageMMCCoefficient;
            }
            else if (nodeData.damageMMCType == MMCType.LevelBased)
            {
                // 基于等级的 MMC
                return nodeData.damageFixedValue * (1 + Level * 0.1f);
            }

            return nodeData.damageFixedValue;
        }
    }
}
