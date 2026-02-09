using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 冷却效果节点
    /// 支持普通CD和充能CD两种模式
    /// </summary>
    public class CooldownEffectNode : EffectNode<CooldownEffectNodeData>
    {
        private EnumField cooldownTypeField;

        // 普通CD参数
        private VisualElement normalCDContainer;
        private TextField durationField;

        // 充能CD参数
        private VisualElement chargeCDContainer;
        private IntegerField maxChargesField;
        private TextField chargeTimeField;

        public CooldownEffectNode(Vector2 position) : base(NodeType.CooldownEffect, position) { }

        protected override string GetNodeTitle() => "冷却";
        protected override float GetNodeWidth() => 150;

        // 我们自己处理持续时间显示
        protected override bool ShowInitialEffectPort => true;
        protected override bool ShowCompleteEffectPort => true;
        protected override void CreateEffectContent()
        {
            // CD类型选择
            cooldownTypeField = new EnumField("CD类型", CooldownType.Normal);
            ApplyFieldStyle(cooldownTypeField);
            cooldownTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.cooldownType = (CooldownType)evt.newValue;
                    NotifyDataChanged();
                }
                UpdateCDTypeDisplay((CooldownType)evt.newValue);
            });
            mainContainer.Add(cooldownTypeField);

            // ========== 普通CD参数容器 ==========
            normalCDContainer = new VisualElement();

            durationField = CreateFormulaField("持续时间", "2", value =>
            {
                if (TypedData != null)
                {
                    TypedData.duration = value;
                    NotifyDataChanged();
                }
            });
            normalCDContainer.Add(durationField);

            mainContainer.Add(normalCDContainer);

            // ========== 充能CD参数容器 ==========
            chargeCDContainer = new VisualElement();

            maxChargesField = new IntegerField("最大充能") { value = 2 };
            ApplyFieldStyle(maxChargesField);
            maxChargesField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.maxCharges = evt.newValue;
                    NotifyDataChanged();
                }
            });
            chargeCDContainer.Add(maxChargesField);

            chargeTimeField = CreateFormulaField("充能时间", "10", value =>
            {
                if (TypedData != null)
                {
                    TypedData.chargeTime = value;
                    NotifyDataChanged();
                }
            });
            chargeCDContainer.Add(chargeTimeField);

            mainContainer.Add(chargeCDContainer);

            // 默认显示普通CD
            UpdateCDTypeDisplay(CooldownType.Normal);
        }

        /// <summary>
        /// 根据CD类型显示/隐藏对应参数
        /// </summary>
        private void UpdateCDTypeDisplay(CooldownType cdType)
        {
            if (normalCDContainer != null)
                normalCDContainer.style.display = cdType == CooldownType.Normal
                    ? DisplayStyle.Flex : DisplayStyle.None;

            if (chargeCDContainer != null)
                chargeCDContainer.style.display = cdType == CooldownType.Charge
                    ? DisplayStyle.Flex : DisplayStyle.None;
        }

        protected override void SyncEffectContentFromData()
        {
            if (TypedData == null) return;

            if (cooldownTypeField != null)
            {
                cooldownTypeField.SetValueWithoutNotify(TypedData.cooldownType);
                UpdateCDTypeDisplay(TypedData.cooldownType);
            }

            // 普通CD参数
            if (durationField != null)
                durationField.SetValueWithoutNotify(TypedData.duration ?? "2");

            // 充能CD参数
            if (maxChargesField != null)
                maxChargesField.SetValueWithoutNotify(TypedData.maxCharges);
            if (chargeTimeField != null)
                chargeTimeField.SetValueWithoutNotify(TypedData.chargeTime ?? "10");
        }
    }
}
