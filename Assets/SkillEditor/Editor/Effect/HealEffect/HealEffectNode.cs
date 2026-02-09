using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 治疗效果节点（瞬时效果）
    /// </summary>
    public class HealEffectNode : EffectNode<HealEffectNodeData>
    {
        // UI字段
        private Label healValueLabel;

        // 输出端口
        private Port completePort;

        public HealEffectNode(Vector2 position) : base(NodeType.HealEffect, position) { }

        protected override string GetNodeTitle() => "治疗";
        protected override float GetNodeWidth() => 150;
        
        protected override bool ShowInitialEffectPort => true;
        protected override bool ShowCompleteEffectPort => true;

        protected override void CreateEffectContent()
        {

            // 治疗值显示标签
            healValueLabel = new Label("治疗值: 10");
            healValueLabel.style.marginLeft = 4;
            healValueLabel.style.marginTop = 4;
            healValueLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            mainContainer.Add(healValueLabel);
        }

        protected override void SyncEffectContentFromData()
        {
            if (TypedData == null) return;
            UpdateHealValueLabel();
        }

        private void UpdateHealValueLabel()
        {
            if (healValueLabel == null || TypedData == null) return;

            string displayValue = TypedData.healSourceType switch
            {
                ModifierMagnitudeSourceType.FixedValue => TypedData.healFixedValue.ToString("F0"),
                ModifierMagnitudeSourceType.Formula => $"公式: {TruncateString(TypedData.healFormula, 12)}",
                ModifierMagnitudeSourceType.ModifierMagnitudeCalculation => $"MMC: {TypedData.healMMCType}",
                ModifierMagnitudeSourceType.SetByCaller => $"上下文: {TruncateString(TypedData.healSetByCallerKey, 10)}",
                _ => "10"
            };
            healValueLabel.text = $"治疗值: {displayValue}";
        }

        private string TruncateString(string str, int maxLength)
        {
            if (string.IsNullOrEmpty(str)) return "未设置";
            return str.Length <= maxLength ? str : str.Substring(0, maxLength) + "...";
        }
    }
}
