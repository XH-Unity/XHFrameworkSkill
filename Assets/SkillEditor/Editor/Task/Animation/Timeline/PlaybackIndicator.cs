using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 播放指示器 - 时间轴上的绿色竖线，支持拖拽跳转
    /// </summary>
    public class PlaybackIndicator : VisualElement
    {
        // 配置
        private const float HANDLE_SIZE = 10f;
        private const float LINE_WIDTH = 2f;

        // 颜色
        private static readonly Color LINE_COLOR = new Color(0.2f, 0.9f, 0.3f, 0.9f);
        private static readonly Color HANDLE_COLOR = new Color(0.2f, 0.9f, 0.3f, 1f);
        private static readonly Color HANDLE_DRAG_COLOR = new Color(0.4f, 1f, 0.5f, 1f);

        // UI元素
        private VisualElement _handle;
        private VisualElement _line;

        // 状态
        private int _currentFrame;
        private bool _isDragging;
        private int _activePointerId = -1;

        // 引用
        private TimelineView _timeline;

        /// <summary>
        /// 当前帧
        /// </summary>
        public int CurrentFrame
        {
            get => _currentFrame;
            set
            {
                _currentFrame = Mathf.Max(0, value);
                UpdatePosition();
            }
        }

        /// <summary>
        /// 拖拽跳转事件（参数为目标帧数）
        /// </summary>
        public event Action<int> OnSeekToFrame;

        public PlaybackIndicator(TimelineView timeline)
        {
            _timeline = timeline;

            name = "PlaybackIndicator";
            style.position = Position.Absolute;
            style.top = 0;
            style.bottom = 0;
            style.width = HANDLE_SIZE;
            pickingMode = PickingMode.Ignore;

            // 创建手柄（顶部小方块）
            _handle = new VisualElement
            {
                name = "PlaybackHandle",
                style =
                {
                    position = Position.Absolute,
                    top = 0,
                    width = HANDLE_SIZE,
                    height = HANDLE_SIZE,
                    backgroundColor = HANDLE_COLOR,
                    borderTopLeftRadius = 2,
                    borderTopRightRadius = 2,
                    borderBottomLeftRadius = 0,
                    borderBottomRightRadius = 0
                },
                pickingMode = PickingMode.Position
            };
            Add(_handle);

            // 创建竖线
            _line = new VisualElement
            {
                name = "PlaybackLine",
                style =
                {
                    position = Position.Absolute,
                    top = HANDLE_SIZE,
                    bottom = 0,
                    left = (HANDLE_SIZE - LINE_WIDTH) / 2f,
                    width = LINE_WIDTH,
                    backgroundColor = LINE_COLOR
                },
                pickingMode = PickingMode.Ignore
            };
            Add(_line);

            // 注册拖拽事件
            _handle.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _handle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _handle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _handle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

            // 默认隐藏
            style.display = DisplayStyle.None;
        }

        /// <summary>
        /// 设置可见性
        /// </summary>
        public void SetVisible(bool visible)
        {
            style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        private void UpdatePosition()
        {
            if (_timeline == null) return;

            float x = _timeline.FrameToPosition(_currentFrame);
            // 偏移使线条居中于手柄
            style.left = TimelineView.TRACK_LABEL_WIDTH + x - HANDLE_SIZE / 2f;
        }

        #region 拖拽事件

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            _isDragging = true;
            _activePointerId = evt.pointerId;
            _handle.CapturePointer(evt.pointerId);
            _handle.style.backgroundColor = HANDLE_DRAG_COLOR;
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!_isDragging || evt.pointerId != _activePointerId) return;

            // 计算鼠标在TrackContent区域中的位置
            float localX = evt.position.x;

            // 将世界坐标转换为相对于PlaybackIndicator父元素的坐标
            var parentWorldBound = parent.worldBound;
            float relativeX = evt.position.x - parentWorldBound.x - TimelineView.TRACK_LABEL_WIDTH;

            float frame = _timeline.PositionToFrame(relativeX);
            int targetFrame = Mathf.Clamp(Mathf.RoundToInt(frame), 0, _timeline.DurationFrames);

            _currentFrame = targetFrame;
            UpdatePosition();
            OnSeekToFrame?.Invoke(targetFrame);

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (!_isDragging || evt.pointerId != _activePointerId) return;

            _handle.ReleasePointer(evt.pointerId);
            _handle.style.backgroundColor = HANDLE_COLOR;
            _isDragging = false;
            _activePointerId = -1;

            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            _handle.style.backgroundColor = HANDLE_COLOR;
            _isDragging = false;
            _activePointerId = -1;
        }

        #endregion
    }
}
