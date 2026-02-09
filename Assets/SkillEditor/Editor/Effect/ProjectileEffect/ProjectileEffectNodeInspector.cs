using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 投射物效果节点Inspector
    /// </summary>
    public class ProjectileEffectNodeInspector : EffectNodeInspector
    {
        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is ProjectileEffectNode projectileNode)
            {
                var data = projectileNode.TypedData;
                if (data == null) return;

                // ============ 发射设置 ============
                var launchSection = CreateCollapsibleSection("发射设置", out var launchContent, true);

                // 发射位置来源
                var launchSourceField = new EnumField("发射位置来源", data.launchPositionSource);
                ApplyEnumFieldStyle(launchSourceField);
                launchSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.launchPositionSource = (PositionSourceType)evt.newValue;
                    projectileNode.SyncUIFromData();
                });
                launchContent.Add(launchSourceField);

                // 发射挂点
                launchContent.Add(CreateTextField("发射挂点", data.launchBindingName, value =>
                {
                    data.launchBindingName = value;
                    projectileNode.SyncUIFromData();
                }));

                container.Add(launchSection);

                // ============ 目标设置 ============
                var targetSection = CreateCollapsibleSection("目标设置", out var targetContent, true);

                // 目标位置来源
                var targetSourceField = new EnumField("目标位置来源", data.targetPositionSource);
                ApplyEnumFieldStyle(targetSourceField);
                targetSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.targetPositionSource = (PositionSourceType)evt.newValue;
                    projectileNode.SyncUIFromData();
                });
                targetContent.Add(targetSourceField);

                // 目标受击挂点
                targetContent.Add(CreateTextField("目标挂点", data.targetBindingName, value =>
                {
                    data.targetBindingName = value;
                    projectileNode.SyncUIFromData();
                }));

                // 投射物目标类型
                var targetTypeField = new EnumField("目标类型", data.projectileTargetType);
                ApplyEnumFieldStyle(targetTypeField);
                targetContent.Add(targetTypeField);

                // 点模式专用参数容器
                var positionModeContainer = new VisualElement();
                targetContent.Add(positionModeContainer);

                // 曲线高度容器
                var curveHeightContainer = new VisualElement();
                targetContent.Add(curveHeightContainer);

                void UpdateTargetTypeUI(ProjectileTargetType targetType)
                {
                    positionModeContainer.Clear();
                    curveHeightContainer.Clear();

                    if (targetType == ProjectileTargetType.Position)
                    {
                        // 点模式：显示飞跃和偏移角度
                        var flyOverToggle = new Toggle("飞跃（飞过目标继续飞）") { value = data.flyOver };
                        flyOverToggle.style.marginTop = 4;
                        flyOverToggle.RegisterValueChangedCallback(evt =>
                        {
                            data.flyOver = evt.newValue;
                            projectileNode.SyncUIFromData();
                            // 飞跃时不显示曲线高度
                            UpdateCurveHeightVisibility(targetType, evt.newValue);
                        });
                        positionModeContainer.Add(flyOverToggle);

                        positionModeContainer.Add(CreateFloatField("偏移角度", data.offsetAngle, value =>
                        {
                            data.offsetAngle = value;
                            projectileNode.SyncUIFromData();
                        }));

                        // 根据飞跃状态显示曲线高度
                        UpdateCurveHeightVisibility(targetType, data.flyOver);
                    }
                    else
                    {
                        // 单位模式：始终显示曲线高度
                        UpdateCurveHeightVisibility(targetType, false);
                    }
                }

                void UpdateCurveHeightVisibility(ProjectileTargetType targetType, bool flyOver)
                {
                    curveHeightContainer.Clear();

                    // 单位模式始终显示，点模式非飞跃时显示
                    if (targetType == ProjectileTargetType.Unit || !flyOver)
                    {
                        curveHeightContainer.Add(CreateFloatField("曲线高度", data.curveHeight, value =>
                        {
                            data.curveHeight = value;
                            projectileNode.SyncUIFromData();
                        }));
                    }
                }

                targetTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.projectileTargetType = (ProjectileTargetType)evt.newValue;
                    UpdateTargetTypeUI((ProjectileTargetType)evt.newValue);
                    projectileNode.SyncUIFromData();
                });

                UpdateTargetTypeUI(data.projectileTargetType);

                container.Add(targetSection);

                // ============ 投射物属性 ============
                var projectileSection = CreateCollapsibleSection("投射物属性", out var projectileContent, true);

                // 预制体
                var prefabField = new ObjectField("预制体") { objectType = typeof(GameObject) };
                prefabField.value = data.projectilePrefab;
                prefabField.RegisterValueChangedCallback(evt =>
                {
                    data.projectilePrefab = evt.newValue as GameObject;
                    projectileNode.SyncUIFromData();
                });
                projectileContent.Add(prefabField);

                // 飞行速度
                projectileContent.Add(CreateFloatField("飞行速度", data.speed, value =>
                {
                    data.speed = value;
                    projectileNode.SyncUIFromData();
                }));

                // 最大距离容器（点模式显示）
                var maxDistanceContainer = new VisualElement();
                projectileContent.Add(maxDistanceContainer);

                void UpdateMaxDistanceVisibility(ProjectileTargetType targetType)
                {
                    maxDistanceContainer.Clear();
                    if (targetType == ProjectileTargetType.Position)
                    {
                        maxDistanceContainer.Add(CreateFloatField("最大距离(-1无限)", data.maxDistance, value =>
                        {
                            data.maxDistance = value;
                            projectileNode.SyncUIFromData();
                        }));
                    }
                }

                // 监听目标类型变化来更新最大距离显示
                targetTypeField.RegisterValueChangedCallback(evt =>
                {
                    UpdateMaxDistanceVisibility((ProjectileTargetType)evt.newValue);
                });
                UpdateMaxDistanceVisibility(data.projectileTargetType);

                // 碰撞半径
                projectileContent.Add(CreateFloatField("碰撞半径", data.collisionRadius, value =>
                {
                    data.collisionRadius = value;
                    projectileNode.SyncUIFromData();
                }));

                container.Add(projectileSection);

                // ============ 穿透设置 ============
                var pierceSection = CreateCollapsibleSection("穿透设置", out var pierceContent, true);

                var isPiercingToggle = new Toggle("是否穿透") { value = data.isPiercing };
                pierceContent.Add(isPiercingToggle);

                // 穿透数量容器
                var pierceCountContainer = new VisualElement();
                pierceContent.Add(pierceCountContainer);

                void UpdatePierceCountVisibility(bool isPiercing)
                {
                    pierceCountContainer.Clear();
                    if (isPiercing)
                    {
                        pierceCountContainer.Add(CreateIntField("最大穿透数", data.maxPierceCount, value =>
                        {
                            data.maxPierceCount = value;
                            projectileNode.SyncUIFromData();
                        }));
                    }
                }

                isPiercingToggle.RegisterValueChangedCallback(evt =>
                {
                    data.isPiercing = evt.newValue;
                    UpdatePierceCountVisibility(evt.newValue);
                    projectileNode.SyncUIFromData();
                });

                UpdatePierceCountVisibility(data.isPiercing);

                container.Add(pierceSection);

                // ============ 碰撞标签 ============
                var collisionTagSection = CreateCollapsibleSection("碰撞标签", out var collisionTagContent, false);

                collisionTagContent.Add(CreateTagSetField("碰撞目标标签", data.collisionTargetTags, value =>
                {
                    data.collisionTargetTags = value;
                    projectileNode.SyncUIFromData();
                }));

                collisionTagContent.Add(CreateTagSetField("碰撞排除标签", data.collisionExcludeTags, value =>
                {
                    data.collisionExcludeTags = value;
                    projectileNode.SyncUIFromData();
                }));

                container.Add(collisionTagSection);
            }
        }
    }
}
