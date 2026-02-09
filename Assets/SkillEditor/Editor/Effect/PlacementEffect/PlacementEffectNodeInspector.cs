using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 放置物效果节点Inspector
    /// </summary>
    public class PlacementEffectNodeInspector : EffectNodeInspector
    {
        // 放置物是持续效果，显示持续时间配置
        protected override bool ShowDurationConfig => true;

        // 放置物支持周期执行（如持续伤害区域）
        protected override bool ShowPeriodicConfig => true;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is PlacementEffectNode placementNode)
            {
                var data = placementNode.TypedData;
                if (data == null) return;

                // ============ 位置设置（紧跟节点目标） ============
                var positionSection = CreateCollapsibleSection("位置设置", out var positionContent, true);

                // 位置来源
                var positionSourceField = new EnumField("位置来源", data.positionSource);
                ApplyEnumFieldStyle(positionSourceField);
                positionSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.positionSource = (PositionSourceType)evt.newValue;
                    placementNode.SyncUIFromData();
                });
                positionContent.Add(positionSourceField);

                // 挂点
                positionContent.Add(CreateTextField("挂点", data.positionBindingName, value =>
                {
                    data.positionBindingName = value;
                    placementNode.SyncUIFromData();
                }));

                container.Add(positionSection);

                // ============ 放置物设置 ============
                var placementSection = CreateCollapsibleSection("放置物设置", out var placementContent, true);

                // 预制体
                var prefabField = new ObjectField("放置物预制体") { objectType = typeof(GameObject) };
                prefabField.value = data.placementPrefab;
                prefabField.RegisterValueChangedCallback(evt =>
                {
                    data.placementPrefab = evt.newValue as GameObject;
                    placementNode.SyncUIFromData();
                });
                placementContent.Add(prefabField);

                container.Add(placementSection);

                // ============ 碰撞设置 ============
                var collisionSection = CreateCollapsibleSection("碰撞设置", out var collisionContent, true);

                // 是否启用碰撞检测
                var enableCollisionToggle = new Toggle("启用碰撞检测") { value = data.enableCollision };
                enableCollisionToggle.tooltip = "启用后，放置物会检测范围内的目标并触发进入/离开事件";
                collisionContent.Add(enableCollisionToggle);

                // 碰撞参数容器（根据开关显示/隐藏）
                var collisionParamsContainer = new VisualElement();
                collisionParamsContainer.style.marginTop = 4;
                collisionContent.Add(collisionParamsContainer);

                void UpdateCollisionParamsVisibility(bool enableCollision)
                {
                    collisionParamsContainer.Clear();
                    if (enableCollision)
                    {
                        // 碰撞半径
                        collisionParamsContainer.Add(CreateFloatField("碰撞半径", data.collisionRadius, value =>
                        {
                            data.collisionRadius = value;
                            placementNode.SyncUIFromData();
                        }));

                        // 碰撞目标标签
                        collisionParamsContainer.Add(CreateTagSetField("碰撞目标标签", data.collisionTargetTags, value =>
                        {
                            data.collisionTargetTags = value;
                            placementNode.SyncUIFromData();
                        }));

                        // 碰撞排除标签
                        collisionParamsContainer.Add(CreateTagSetField("碰撞排除标签", data.collisionExcludeTags, value =>
                        {
                            data.collisionExcludeTags = value;
                            placementNode.SyncUIFromData();
                        }));
                    }
                }

                enableCollisionToggle.RegisterValueChangedCallback(evt =>
                {
                    data.enableCollision = evt.newValue;
                    UpdateCollisionParamsVisibility(evt.newValue);
                    placementNode.SyncUIFromData();
                });

                // 初始化显示
                UpdateCollisionParamsVisibility(data.enableCollision);

                container.Add(collisionSection);
            }
        }
    }
}
