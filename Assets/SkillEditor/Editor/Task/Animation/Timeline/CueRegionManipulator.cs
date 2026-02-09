using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Cue区域拖拽操作器 - 使用更简单直接的方式
    /// </summary>
    public class CueRegionManipulator
    {
        private enum DragMode { None, Move, ResizeLeft, ResizeRight }

        private DragMode _dragMode = DragMode.None;
        private Vector2 _startMousePosition;
        private float _originalStartTime;
        private float _originalEndTime;
        private int _activePointerId = -1;

        private readonly TimeCueTrack _track;
        private readonly VisualElement _region;
        private readonly VisualElement _leftHandle;
        private readonly VisualElement _rightHandle;

        public CueRegionManipulator(TimeCueTrack track, VisualElement region,
            VisualElement leftHandle, VisualElement rightHandle)
        {
            _track = track;
            _region = region;
            _leftHandle = leftHandle;
            _rightHandle = rightHandle;

            RegisterCallbacks();
        }

        private void RegisterCallbacks()
        {
            // 左手柄事件
            _leftHandle.RegisterCallback<PointerDownEvent>(OnLeftHandlePointerDown);
            _leftHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _leftHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _leftHandle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

            // 右手柄事件
            _rightHandle.RegisterCallback<PointerDownEvent>(OnRightHandlePointerDown);
            _rightHandle.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _rightHandle.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _rightHandle.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);

            // 区域中间事件（整体移动）
            _region.RegisterCallback<PointerDownEvent>(OnRegionPointerDown);
            _region.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _region.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _region.RegisterCallback<PointerCaptureOutEvent>(OnPointerCaptureOut);
        }

        private void OnLeftHandlePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            _dragMode = DragMode.ResizeLeft;
            _startMousePosition = evt.position;
            _originalStartTime = _track.GetStartTime();
            _originalEndTime = _track.GetEndTime();
            _activePointerId = evt.pointerId;

            _leftHandle.CapturePointer(evt.pointerId);
            _leftHandle.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);
            evt.StopPropagation();
        }

        private void OnRightHandlePointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            _dragMode = DragMode.ResizeRight;
            _startMousePosition = evt.position;
            _originalStartTime = _track.GetStartTime();
            _originalEndTime = _track.GetEndTime();
            _activePointerId = evt.pointerId;

            _rightHandle.CapturePointer(evt.pointerId);
            _rightHandle.style.backgroundColor = new Color(1f, 1f, 1f, 0.9f);
            evt.StopPropagation();
        }

        private void OnRegionPointerDown(PointerDownEvent evt)
        {
            if (evt.button != 0) return;

            // 检查是否点击在手柄上（如果是则不处理）
            if (evt.target == _leftHandle || evt.target == _rightHandle)
                return;

            _dragMode = DragMode.Move;
            _startMousePosition = evt.position;
            _originalStartTime = _track.GetStartTime();
            _originalEndTime = _track.GetEndTime();
            _activePointerId = evt.pointerId;

            _region.CapturePointer(evt.pointerId);
            _region.style.backgroundColor = new Color(0.3f, 0.8f, 0.5f, 0.9f);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (_dragMode == DragMode.None || evt.pointerId != _activePointerId)
                return;

            Vector2 delta = (Vector2)evt.position - _startMousePosition;
            float deltaFrame = Mathf.Round(_track.Timeline.PositionToFrame(delta.x));

            switch (_dragMode)
            {
                case DragMode.Move:
                    _track.MoveRegion(_originalStartTime, _originalEndTime, deltaFrame);
                    break;

                case DragMode.ResizeLeft:
                    _track.ResizeLeft(_originalStartTime, _originalEndTime, deltaFrame);
                    break;

                case DragMode.ResizeRight:
                    _track.ResizeRight(_originalStartTime, _originalEndTime, deltaFrame);
                    break;
            }

            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (_dragMode == DragMode.None || evt.pointerId != _activePointerId)
                return;

            // 恢复颜色
            _leftHandle.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
            _rightHandle.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
            _region.style.backgroundColor = new Color(0.2f, 0.7f, 0.4f, 0.8f);

            // 释放指针
            switch (_dragMode)
            {
                case DragMode.ResizeLeft:
                    _leftHandle.ReleasePointer(evt.pointerId);
                    break;
                case DragMode.ResizeRight:
                    _rightHandle.ReleasePointer(evt.pointerId);
                    break;
                case DragMode.Move:
                    _region.ReleasePointer(evt.pointerId);
                    break;
            }

            _dragMode = DragMode.None;
            _activePointerId = -1;

            // 通知数据变化
            _track.NotifyDataChanged();

            evt.StopPropagation();
        }

        private void OnPointerCaptureOut(PointerCaptureOutEvent evt)
        {
            if (_dragMode == DragMode.None)
                return;

            // 恢复颜色
            _leftHandle.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
            _rightHandle.style.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);
            _region.style.backgroundColor = new Color(0.2f, 0.7f, 0.4f, 0.8f);

            _dragMode = DragMode.None;
            _activePointerId = -1;
        }
    }
}
