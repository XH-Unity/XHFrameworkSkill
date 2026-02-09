using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 位移效果节点Inspector
    /// </summary>
    public class DisplaceEffectNodeInspector : EffectNodeInspector
    {
        // 位移效果不需要周期/堆叠/属性修改器配置
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => false;
        protected override bool ShowAttributeModifiers => false;
        protected override bool ShowStackConfig => false;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is DisplaceEffectNode displaceNode)
            {
                var data = displaceNode.TypedData;
                if (data == null) return;

                // ============ 位移配置 ============
                var displaceSection = CreateCollapsibleSection("位移配置", out var displaceContent, true);

                // 位移类型
                var displaceTypeField = new EnumField("位移类型", data.displaceType);
                ApplyEnumFieldStyle(displaceTypeField);
                displaceContent.Add(displaceTypeField);

                // 速度
                displaceContent.Add(CreateFloatField("速度（单位/秒）", data.speed, value =>
                {
                    data.speed = value;
                    displaceNode.SyncUIFromData();
                }));

                // 最大距离
                displaceContent.Add(CreateFloatField("最大距离", data.distance, value =>
                {
                    data.distance = value;
                    displaceNode.SyncUIFromData();
                }));

                // 动态容器：根据位移类型显示/隐藏条件字段
                var dynamicContainer = new VisualElement();
                displaceContent.Add(dynamicContainer);

                container.Add(displaceSection);

                // 动态容器：PullToPoint 目标点配置
                var pointContainer = new VisualElement();
                container.Add(pointContainer);

                void UpdateDynamicFields(DisplaceType type)
                {
                    dynamicContainer.Clear();
                    pointContainer.Clear();

                    // 最小距离（吸引模式时显示）
                    if (type == DisplaceType.Pull || type == DisplaceType.PullToPoint)
                    {
                        dynamicContainer.Add(CreateFloatField("最小距离（到达后停止）", data.minDistance, value =>
                        {
                            data.minDistance = value;
                            displaceNode.SyncUIFromData();
                        }));
                    }

                    // PullToPoint 目标点配置
                    if (type == DisplaceType.PullToPoint)
                    {
                        var pointSection = CreateCollapsibleSection("目标点配置", out var pointContent, true);

                        // 位置来源
                        var pointSourceField = new EnumField("位置来源", data.pointSource);
                        ApplyEnumFieldStyle(pointSourceField);
                        pointSourceField.RegisterValueChangedCallback(evt =>
                        {
                            data.pointSource = (PositionSourceType)evt.newValue;
                            displaceNode.SyncUIFromData();
                        });
                        pointContent.Add(pointSourceField);

                        // 挂点名称
                        pointContent.Add(CreateTextField("挂点名称", data.pointBindingName, value =>
                        {
                            data.pointBindingName = value;
                            displaceNode.SyncUIFromData();
                        }));

                        pointContainer.Add(pointSection);
                    }
                }

                displaceTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.displaceType = (DisplaceType)evt.newValue;
                    displaceNode.SyncUIFromData();
                    UpdateDynamicFields((DisplaceType)evt.newValue);
                });

                // 初始化动态字段
                UpdateDynamicFields(data.displaceType);
            }
        }
    }
}
