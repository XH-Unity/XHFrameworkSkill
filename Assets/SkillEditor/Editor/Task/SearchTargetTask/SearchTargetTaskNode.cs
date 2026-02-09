using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 搜索目标任务节点 - 搜索范围内的目标并遍历执行
    /// </summary>
    public class SearchTargetTaskNode : TaskNode<SearchTargetTaskNodeData>
    {
        // UI控件
        private EnumField searchShapeField;
        private EnumField lineTypeField;
        private IntegerField maxTargetsField;
        private Port foreachPort;
        private Port completePort;

        // 形状参数容器
        private VisualElement shapeParamsContainer;
        private VisualElement lineTypeContainer;
        private VisualElement lineParamsContainer;

        // 圆形参数
        private FloatField circleRadiusField;

        // 扇形参数
        private FloatField sectorRadiusField;
        private FloatField sectorAngleField;

        // 直线参数
        private FloatField lineOffsetAngleField;
        private FloatField lineWidthField;
        private FloatField lineLengthField;
        private EnumField lineStartPointField;
        private EnumField lineEndPointField;
        private FloatField lineAbsoluteAngleField;

        public SearchTargetTaskNode(Vector2 position) : base(NodeType.SearchTargetTask, position) { }

        protected override string GetNodeTitle() => "搜索目标";
        protected override float GetNodeWidth() => 200;

        protected override void CreateTaskContent()
        {
            foreachPort = CreateOutputPort("对每个目标");
            completePort = CreateOutputPort("完成效果");

            // 搜索形状
            searchShapeField = new EnumField("搜索形状", SearchShapeType.Circle);
            ApplyFieldStyle(searchShapeField);
            searchShapeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchShapeType = (SearchShapeType)evt.newValue;
                    NotifyDataChanged();
                }
                OnSearchShapeChanged((SearchShapeType)evt.newValue);
            });
            mainContainer.Add(searchShapeField);

            // 形状参数容器
            shapeParamsContainer = new VisualElement();
            mainContainer.Add(shapeParamsContainer);

            // 创建所有形状参数
            CreateCircleParams();
            CreateSectorParams();
            CreateLineParams();

            // 默认显示圆形参数
            OnSearchShapeChanged(SearchShapeType.Circle);

            // 最大目标数
            maxTargetsField = new IntegerField("最大目标数") { value = 0 };
            ApplyFieldStyle(maxTargetsField);
            maxTargetsField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.maxTargets = evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(maxTargetsField);
        }

        private void CreateCircleParams()
        {
            circleRadiusField = new FloatField("半径") { value = 5f };
            ApplyFieldStyle(circleRadiusField);
            circleRadiusField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchCircleRadius = evt.newValue;
                    NotifyDataChanged();
                }
            });
        }

        private void CreateSectorParams()
        {
            sectorRadiusField = new FloatField("半径") { value = 5f };
            ApplyFieldStyle(sectorRadiusField);
            sectorRadiusField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchSectorRadius = evt.newValue;
                    NotifyDataChanged();
                }
            });

            sectorAngleField = new FloatField("角度") { value = 90f };
            ApplyFieldStyle(sectorAngleField);
            sectorAngleField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchSectorAngle = evt.newValue;
                    NotifyDataChanged();
                }
            });
        }

        private void CreateLineParams()
        {
            lineTypeContainer = new VisualElement();

            lineTypeField = new EnumField("直线类型", LineType.UnitDirection);
            ApplyFieldStyle(lineTypeField);
            lineTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchLineType = (LineType)evt.newValue;
                    NotifyDataChanged();
                }
                OnLineTypeChanged((LineType)evt.newValue);
            });
            lineTypeContainer.Add(lineTypeField);

            lineParamsContainer = new VisualElement();
            lineTypeContainer.Add(lineParamsContainer);

            // 通用直线参数
            lineOffsetAngleField = new FloatField("偏移角度") { value = 0f };
            ApplyFieldStyle(lineOffsetAngleField);
            lineOffsetAngleField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchLineDirectionOffsetAngle = evt.newValue;
                    NotifyDataChanged();
                }
            });

            lineWidthField = new FloatField("宽度") { value = 1f };
            ApplyFieldStyle(lineWidthField);
            lineWidthField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchLineDirectionWidth = evt.newValue;
                    NotifyDataChanged();
                }
            });

            lineLengthField = new FloatField("长度") { value = 10f };
            ApplyFieldStyle(lineLengthField);
            lineLengthField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchLineDirectionLength = evt.newValue;
                    NotifyDataChanged();
                }
            });

            // 两点之间参数
            lineStartPointField = new EnumField("起点", PositionSourceType.Caster);
            ApplyFieldStyle(lineStartPointField);
            lineStartPointField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.lineStartPositionSource = (PositionSourceType)evt.newValue;
                    NotifyDataChanged();
                }
            });

            lineEndPointField = new EnumField("终点", PositionSourceType.MainTarget);
            ApplyFieldStyle(lineEndPointField);
            lineEndPointField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.lineEndPositionSource = (PositionSourceType)evt.newValue;
                    NotifyDataChanged();
                }
            });

            // 绝对角度参数
            lineAbsoluteAngleField = new FloatField("绝对角度") { value = 0f };
            ApplyFieldStyle(lineAbsoluteAngleField);
            lineAbsoluteAngleField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.searchLineAbsoluteAngle = evt.newValue;
                    NotifyDataChanged();
                }
            });
        }

        private void OnSearchShapeChanged(SearchShapeType shapeType)
        {
            shapeParamsContainer.Clear();

            switch (shapeType)
            {
                case SearchShapeType.Circle:
                    shapeParamsContainer.Add(circleRadiusField);
                    break;
                case SearchShapeType.Sector:
                    shapeParamsContainer.Add(sectorRadiusField);
                    shapeParamsContainer.Add(sectorAngleField);
                    break;
                case SearchShapeType.Line:
                    shapeParamsContainer.Add(lineTypeContainer);
                    OnLineTypeChanged((LineType)lineTypeField.value);
                    break;
            }
        }

        private void OnLineTypeChanged(LineType lineType)
        {
            lineParamsContainer.Clear();

            switch (lineType)
            {
                case LineType.UnitDirection:
                    lineParamsContainer.Add(lineOffsetAngleField);
                    lineParamsContainer.Add(lineWidthField);
                    lineParamsContainer.Add(lineLengthField);
                    break;
                case LineType.BetweenPoints:
                    lineParamsContainer.Add(lineStartPointField);
                    lineParamsContainer.Add(lineEndPointField);
                    lineParamsContainer.Add(lineWidthField);
                    break;
                case LineType.AbsoluteAngle:
                    lineParamsContainer.Add(lineAbsoluteAngleField);
                    lineParamsContainer.Add(lineWidthField);
                    lineParamsContainer.Add(lineLengthField);
                    break;
            }
        }

        protected override void SyncTaskContentFromData()
        {
            if (TypedData == null) return;

            if (searchShapeField != null)
            {
                searchShapeField.SetValueWithoutNotify(TypedData.searchShapeType);
                OnSearchShapeChanged(TypedData.searchShapeType);
            }
            if (lineTypeField != null)
            {
                lineTypeField.SetValueWithoutNotify(TypedData.searchLineType);
                OnLineTypeChanged(TypedData.searchLineType);
            }
            if (maxTargetsField != null) maxTargetsField.SetValueWithoutNotify(TypedData.maxTargets);

            // 同步形状参数
            if (circleRadiusField != null) circleRadiusField.SetValueWithoutNotify(TypedData.searchCircleRadius);
            if (sectorRadiusField != null) sectorRadiusField.SetValueWithoutNotify(TypedData.searchSectorRadius);
            if (sectorAngleField != null) sectorAngleField.SetValueWithoutNotify(TypedData.searchSectorAngle);
            if (lineOffsetAngleField != null) lineOffsetAngleField.SetValueWithoutNotify(TypedData.searchLineDirectionOffsetAngle);
            if (lineWidthField != null) lineWidthField.SetValueWithoutNotify(TypedData.searchLineDirectionWidth);
            if (lineLengthField != null) lineLengthField.SetValueWithoutNotify(TypedData.searchLineDirectionLength);
            if (lineStartPointField != null) lineStartPointField.SetValueWithoutNotify(TypedData.lineStartPositionSource);
            if (lineEndPointField != null) lineEndPointField.SetValueWithoutNotify(TypedData.lineEndPositionSource);
            if (lineAbsoluteAngleField != null) lineAbsoluteAngleField.SetValueWithoutNotify(TypedData.searchLineAbsoluteAngle);
        }
    }
}
