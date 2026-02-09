using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 伤害效果节点（瞬时效果）
    /// </summary>
    public class DamageEffectNode : EffectNode<DamageEffectNodeData>
    {
        // UI字段
        private EnumField damageTypeField;
        private Label damageValueLabel;

    

        public DamageEffectNode(Vector2 position) : base(NodeType.DamageEffect, position) { }

        protected override string GetNodeTitle() => "伤害";
        protected override float GetNodeWidth() => 150;
        protected override bool ShowCompleteEffectPort => true;
        protected override void CreateEffectContent()
        {
            // 创建输出端口
           

            // 伤害类型
            damageTypeField = new EnumField("伤害类型", DamageType.Physical);
            ApplyFieldStyle(damageTypeField);
            var label = damageTypeField.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = 50;
                label.style.width = 50;
            }
            damageTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.damageType = (DamageType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(damageTypeField);

            // 伤害值显示标签
            damageValueLabel = new Label("伤害值: 10");
            damageValueLabel.style.marginLeft = 4;
            damageValueLabel.style.marginTop = 4;
            damageValueLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            mainContainer.Add(damageValueLabel);
        }

        protected override void SyncEffectContentFromData()
        {
            if (TypedData == null) return;

            if (damageTypeField != null) damageTypeField.SetValueWithoutNotify(TypedData.damageType);
            UpdateDamageValueLabel();
        }

        private void UpdateDamageValueLabel()
        {
            if (damageValueLabel == null || TypedData == null) return;

            string displayValue = TypedData.damageSourceType switch
            {
                ModifierMagnitudeSourceType.FixedValue => TypedData.damageFixedValue.ToString("F0"),
                ModifierMagnitudeSourceType.Formula => $"公式: {TruncateString(TypedData.damageFormula, 12)}",
                ModifierMagnitudeSourceType.ModifierMagnitudeCalculation => $"MMC: {TypedData.damageMMCType}",
                ModifierMagnitudeSourceType.SetByCaller => $"上下文: {TruncateString(TypedData.damageSetByCallerKey, 10)}",
                _ => "10"
            };
            damageValueLabel.text = $"伤害值: {displayValue}";
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return "未设置";
            return str.Length <= maxLength ? str : str.Substring(0, maxLength) + "...";
        }
    }
}
