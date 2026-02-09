using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 位移效果节点 - 吸引/击退目标
    /// </summary>
    public class DisplaceEffectNode : EffectNode<DisplaceEffectNodeData>
    {
        private EnumField displaceTypeField;
        private FloatField speedField;
        private FloatField distanceField;

        public DisplaceEffectNode(Vector2 position) : base(NodeType.DisplaceEffect, position) { }

        protected override string GetNodeTitle() => "位移效果";
        protected override float GetNodeWidth() => 180;

        // 位移效果是持续效果，但不需要暴露周期/堆叠/属性修改器
        protected override bool ShowDurationTypeConfig => false;
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => false;
        protected override bool ShowAttributeModifiers => false;
        protected override bool ShowStackConfig => false;

        // 输出端口：完成效果（位移结束后触发）
        protected override bool ShowInitialEffectPort => false;
        protected override bool ShowPeriodicEffectPort => false;
        protected override bool ShowCompleteEffectPort => true;

        protected override void CreateEffectContent()
        {
            // 位移类型
            displaceTypeField = new EnumField("位移类型", DisplaceType.Push);
            ApplyFieldStyle(displaceTypeField);
            displaceTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.displaceType = (DisplaceType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(displaceTypeField);

            // 速度
            speedField = new FloatField("速度") { value = 10f };
            ApplyFieldStyle(speedField);
            speedField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.speed = evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(speedField);

            // 最大距离
            distanceField = new FloatField("最大距离") { value = 5f };
            ApplyFieldStyle(distanceField);
            distanceField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.distance = evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(distanceField);
        }

        protected override void SyncEffectContentFromData()
        {
            if (TypedData == null) return;
            if (displaceTypeField != null)
                displaceTypeField.SetValueWithoutNotify(TypedData.displaceType);
            if (speedField != null)
                speedField.SetValueWithoutNotify(TypedData.speed);
            if (distanceField != null)
                distanceField.SetValueWithoutNotify(TypedData.distance);
        }
    }
}
