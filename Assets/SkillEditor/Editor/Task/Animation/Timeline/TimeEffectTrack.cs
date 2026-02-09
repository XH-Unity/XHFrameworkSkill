using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 时间效果轨道 - 显示为菱形标记，可拖拽改变触发时间
    /// </summary>
    public class TimeEffectTrack : TimelineTrackBase
    {
        private TimeEffectData _data;
        private int _index;
        private VisualElement _marker;
        private Label _nameLabel;  // 保存标签引用以便更新
        private Label _frameLabel; // 菱形上方的帧数标签

        // 拖拽状态
        private bool _isDragging;
        private float _dragStartX;
        private int _originalTime;

        // 配置
        private const float MARKER_WIDTH = 28f;
        private const float MARKER_HEIGHT = 16f;

        // 颜色配置
        private static readonly Color MARKER_COLOR = new Color(1f, 0.6f, 0.2f); // 橙色
        private static readonly Color MARKER_HOVER_COLOR = new Color(1f, 0.7f, 0.4f);

        public TimeEffectTrack(TimelineView timeline, TimeEffectData data, int index, Func<Port> createPortFunc)
            : base(timeline)
        {
            _data = data;
            _index = index;
            PortId = data.portId;

            // 创建删除按钮
            var removeButton = new Button { text = "×" };
            removeButton.style.width = 16;
            removeButton.style.height = 16;
            removeButton.style.paddingLeft = 0;
            removeButton.style.paddingRight = 0;
            removeButton.style.paddingTop = 0;
            removeButton.style.paddingBottom = 0;
            removeButton.style.fontSize = 12;
            removeButton.style.marginRight = 4;
            removeButton.style.backgroundColor = new Color(0.6f, 0.2f, 0.2f);
            removeButton.style.borderTopLeftRadius = 3;
            removeButton.style.borderTopRightRadius = 3;
            removeButton.style.borderBottomLeftRadius = 3;
            removeButton.style.borderBottomRightRadius = 3;
            removeButton.clicked += RequestRemove;
            TrackLabel.Add(removeButton);

            // 创建标签（显示序号）
            _nameLabel = new Label($"效果 {index + 1}")
            {
                style =
                {
                    fontSize = 10,
                    color = MARKER_COLOR,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            TrackLabel.Add(_nameLabel);

            // 创建菱形标记
            CreateMarker();

            // 创建输出端口
            if (createPortFunc != null)
            {
                OutputPort = createPortFunc();
                OutputPort.portName = "";
                OutputPort.portColor = MARKER_COLOR;
                OutputPort.name = data.portId;
                PortArea.Add(OutputPort);

                // 如果是TimelinePort，监听连接变化事件
                if (OutputPort is TimelinePort timelinePort)
                {
                    timelinePort.OnConnectionChanged += UpdateTrackName;
                }
            }

            // 初始布局
            UpdateLayout();
        }

        /// <summary>
        /// 更新轨道名称（根据连接状态）
        /// </summary>
        public void UpdateTrackName()
        {
            if (OutputPort == null || _nameLabel == null) return;

            string baseName = $"效果 {_index + 1}";

            // 检查端口是否有连接
            if (OutputPort.connected)
            {
                // 遍历所有连接
                foreach (var edge in OutputPort.connections)
                {
                    if (edge != null && edge.input != null && edge.input.node != null)
                    {
                        // 获取连接的目标节点名称
                        string targetNodeName = edge.input.node.title;
                        _nameLabel.text = $"{baseName}:{targetNodeName}";
                        return;
                    }
                }
            }

            // 没有连接时显示基础名称
            _nameLabel.text = baseName;
        }

        /// <summary>
        /// 创建水平六边形标记
        /// </summary>
        private void CreateMarker()
        {
            _marker = new VisualElement
            {
                name = "TimeEffectMarker",
                style =
                {
                    position = Position.Absolute,
                    width = MARKER_WIDTH,
                    height = MARKER_HEIGHT,
                    top = (TimelineView.TRACK_HEIGHT - MARKER_HEIGHT) / 2
                }
            };
            _marker.generateVisualContent += OnGenerateMarkerVisual;

            // 注册拖拽事件
            _marker.RegisterCallback<MouseDownEvent>(OnMarkerMouseDown);
            _marker.RegisterCallback<MouseMoveEvent>(OnMarkerMouseMove);
            _marker.RegisterCallback<MouseUpEvent>(OnMarkerMouseUp);
            _marker.RegisterCallback<MouseEnterEvent>(evt =>
            {
                _markerColor = MARKER_HOVER_COLOR;
                _marker.MarkDirtyRepaint();
            });
            _marker.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                if (!_isDragging)
                {
                    _markerColor = MARKER_COLOR;
                    _marker.MarkDirtyRepaint();
                }
            });

            TrackContent.Add(_marker);

            // 帧数标签（显示在六边形正中心，不拦截鼠标事件）
            _frameLabel = new Label
            {
                name = "FrameLabel",
                pickingMode = PickingMode.Ignore,
                style =
                {
                    position = Position.Absolute,
                    fontSize = 9,
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    height = MARKER_HEIGHT,
                    top = (TimelineView.TRACK_HEIGHT - MARKER_HEIGHT) / 2,
                    width = MARKER_WIDTH
                }
            };
            TrackContent.Add(_frameLabel);
        }

        private Color _markerColor = MARKER_COLOR;

        /// <summary>
        /// 绘制水平六边形
        /// </summary>
        private void OnGenerateMarkerVisual(MeshGenerationContext mgc)
        {
            var painter = mgc.painter2D;
            float w = MARKER_WIDTH;
            float h = MARKER_HEIGHT;
            float tipW = h / 2f; // 左右尖角的水平宽度

            painter.fillColor = _markerColor;
            painter.BeginPath();
            // 从左尖角开始，顺时针绘制
            painter.MoveTo(new Vector2(0, h / 2));           // 左尖
            painter.LineTo(new Vector2(tipW, 0));            // 左上
            painter.LineTo(new Vector2(w - tipW, 0));        // 右上
            painter.LineTo(new Vector2(w, h / 2));           // 右尖
            painter.LineTo(new Vector2(w - tipW, h));        // 右下
            painter.LineTo(new Vector2(tipW, h));            // 左下
            painter.ClosePath();
            painter.Fill();
        }

        /// <summary>
        /// 更新布局
        /// </summary>
        public override void UpdateLayout()
        {
            if (_marker == null) return;

            float x = Timeline.FrameToPosition(_data.triggerTime);
            _marker.style.left = x - MARKER_WIDTH / 2; // 居中对齐

            // 更新帧数标签（与六边形完全重叠）
            if (_frameLabel != null)
            {
                _frameLabel.text = $"{_data.triggerTime}";
                _frameLabel.style.left = x - MARKER_WIDTH / 2;
            }
        }

        #region 拖拽逻辑

        private void OnMarkerMouseDown(MouseDownEvent evt)
        {
            if (evt.button != 0) return;

            _isDragging = true;
            _dragStartX = evt.mousePosition.x;
            _originalTime = _data.triggerTime;

            _marker.CaptureMouse();
            _markerColor = MARKER_HOVER_COLOR;
            _marker.MarkDirtyRepaint();
            evt.StopPropagation();
        }

        private void OnMarkerMouseMove(MouseMoveEvent evt)
        {
            if (!_isDragging) return;

            float deltaX = evt.mousePosition.x - _dragStartX;
            float deltaFrame = Mathf.Round(Timeline.PositionToFrame(deltaX));
            int newFrame = (int)Mathf.Max(_originalTime + deltaFrame, 0);

            _data.triggerTime = newFrame;
            UpdateLayout();
        }

        private void OnMarkerMouseUp(MouseUpEvent evt)
        {
            if (!_isDragging) return;

            _isDragging = false;
            _marker.ReleaseMouse();
            _markerColor = MARKER_COLOR;
            _marker.MarkDirtyRepaint();

            // 通知数据变化
            NotifyDataModified();
        }

        #endregion
    }
}
