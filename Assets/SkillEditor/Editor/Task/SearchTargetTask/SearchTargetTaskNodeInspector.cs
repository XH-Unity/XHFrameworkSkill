using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 搜索目标任务节点Inspector
    /// </summary>
    public class SearchTargetTaskNodeInspector : TaskNodeInspector
    {
        // 搜索目标不需要显示节点目标，使用 PositionSourceType
        protected override bool ShowTargetType => false;

        protected override void BuildTaskInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is SearchTargetTaskNode searchNode)
            {
                var data = searchNode.TypedData;
                if (data == null) return;

                // ============ 位置来源设置 ============
                var positionSection = CreateCollapsibleSection("位置设置", out var positionContent, true);

                // 位置来源类型
                var positionSourceField = new EnumField("位置来源", data.positionSource);
                ApplyEnumFieldStyle(positionSourceField);
                positionSourceField.RegisterValueChangedCallback(evt =>
                {
                    data.positionSource = (PositionSourceType)evt.newValue;
                    searchNode.SyncUIFromData();
                });
                positionContent.Add(positionSourceField);

                // 挂点
                positionContent.Add(CreateTextField("挂点", data.positionBindingName, value =>
                {
                    data.positionBindingName = value;
                    searchNode.SyncUIFromData();
                }));

                container.Add(positionSection);

                // ============ 搜索形状设置 ============
                var shapeSection = CreateCollapsibleSection("搜索形状", out var shapeContent, true);

                // 搜索形状
                var shapeField = new EnumField("形状类型", data.searchShapeType);
                ApplyEnumFieldStyle(shapeField);
                shapeContent.Add(shapeField);

                // 形状参数容器
                var shapeParamsContainer = new VisualElement();
                shapeContent.Add(shapeParamsContainer);

                void UpdateShapeParams(SearchShapeType shapeType)
                {
                    shapeParamsContainer.Clear();

                    switch (shapeType)
                    {
                        case SearchShapeType.Circle:
                            shapeParamsContainer.Add(CreateFloatField("半径", data.searchCircleRadius, value =>
                            {
                                data.searchCircleRadius = value;
                                searchNode.SyncUIFromData();
                            }));
                            break;

                        case SearchShapeType.Sector:
                            shapeParamsContainer.Add(CreateFloatField("半径", data.searchSectorRadius, value =>
                            {
                                data.searchSectorRadius = value;
                                searchNode.SyncUIFromData();
                            }));
                            shapeParamsContainer.Add(CreateFloatField("角度", data.searchSectorAngle, value =>
                            {
                                data.searchSectorAngle = value;
                                searchNode.SyncUIFromData();
                            }));
                            break;

                        case SearchShapeType.Line:
                            var lineTypeField = new EnumField("直线类型", data.searchLineType);
                            ApplyEnumFieldStyle(lineTypeField);
                            shapeParamsContainer.Add(lineTypeField);

                            var lineParamsContainer = new VisualElement();
                            shapeParamsContainer.Add(lineParamsContainer);

                            void UpdateLineParams(LineType lineType)
                            {
                                lineParamsContainer.Clear();

                                switch (lineType)
                                {
                                    case LineType.UnitDirection:
                                        lineParamsContainer.Add(CreateFloatField("偏移角度", data.searchLineDirectionOffsetAngle, value =>
                                        {
                                            data.searchLineDirectionOffsetAngle = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        lineParamsContainer.Add(CreateFloatField("宽度", data.searchLineDirectionWidth, value =>
                                        {
                                            data.searchLineDirectionWidth = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        lineParamsContainer.Add(CreateFloatField("长度", data.searchLineDirectionLength, value =>
                                        {
                                            data.searchLineDirectionLength = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        break;

                                    case LineType.BetweenPoints:
                                        // 起点位置来源
                                        var startSourceField = new EnumField("起点来源", data.lineStartPositionSource);
                                        ApplyEnumFieldStyle(startSourceField);
                                        startSourceField.RegisterValueChangedCallback(evt =>
                                        {
                                            data.lineStartPositionSource = (PositionSourceType)evt.newValue;
                                            searchNode.SyncUIFromData();
                                        });
                                        lineParamsContainer.Add(startSourceField);

                                        lineParamsContainer.Add(CreateTextField("起点挂点", data.lineStartBindingName, value =>
                                        {
                                            data.lineStartBindingName = value;
                                            searchNode.SyncUIFromData();
                                        }));

                                        // 终点位置来源
                                        var endSourceField = new EnumField("终点来源", data.lineEndPositionSource);
                                        ApplyEnumFieldStyle(endSourceField);
                                        endSourceField.RegisterValueChangedCallback(evt =>
                                        {
                                            data.lineEndPositionSource = (PositionSourceType)evt.newValue;
                                            searchNode.SyncUIFromData();
                                        });
                                        lineParamsContainer.Add(endSourceField);

                                        lineParamsContainer.Add(CreateTextField("终点挂点", data.lineEndBindingName, value =>
                                        {
                                            data.lineEndBindingName = value;
                                            searchNode.SyncUIFromData();
                                        }));

                                        lineParamsContainer.Add(CreateFloatField("宽度", data.searchLineBetweenWidth, value =>
                                        {
                                            data.searchLineBetweenWidth = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        break;

                                    case LineType.AbsoluteAngle:
                                        lineParamsContainer.Add(CreateFloatField("绝对角度", data.searchLineAbsoluteAngle, value =>
                                        {
                                            data.searchLineAbsoluteAngle = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        lineParamsContainer.Add(CreateFloatField("宽度", data.searchLineAbsoluteWidth, value =>
                                        {
                                            data.searchLineAbsoluteWidth = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        lineParamsContainer.Add(CreateFloatField("长度", data.searchLineAbsoluteLength, value =>
                                        {
                                            data.searchLineAbsoluteLength = value;
                                            searchNode.SyncUIFromData();
                                        }));
                                        break;
                                }
                            }

                            lineTypeField.RegisterValueChangedCallback(evt =>
                            {
                                data.searchLineType = (LineType)evt.newValue;
                                UpdateLineParams((LineType)evt.newValue);
                                searchNode.SyncUIFromData();
                            });

                            UpdateLineParams(data.searchLineType);
                            break;
                    }
                }

                shapeField.RegisterValueChangedCallback(evt =>
                {
                    data.searchShapeType = (SearchShapeType)evt.newValue;
                    UpdateShapeParams((SearchShapeType)evt.newValue);
                    searchNode.SyncUIFromData();
                });

                UpdateShapeParams(data.searchShapeType);

                container.Add(shapeSection);

                // ============ 搜索参数 ============
                var searchSection = CreateCollapsibleSection("搜索参数", out var searchContent, true);

                // 最大目标数
                searchContent.Add(CreateIntField("最大目标数", data.maxTargets, value =>
                {
                    data.maxTargets = value;
                    searchNode.SyncUIFromData();
                }));

                // 搜索目标标签
                searchContent.Add(CreateTagSetField("目标标签", data.searchTargetTags, value =>
                {
                    data.searchTargetTags = value;
                    searchNode.SyncUIFromData();
                }));

                // 排除标签
                searchContent.Add(CreateTagSetField("排除标签", data.searchExcludeTags, value =>
                {
                    data.searchExcludeTags = value;
                    searchNode.SyncUIFromData();
                }));

                container.Add(searchSection);
            }
        }
    }
}
