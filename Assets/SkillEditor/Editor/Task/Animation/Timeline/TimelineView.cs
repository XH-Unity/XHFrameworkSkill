using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Timeline主容器 - 管理时间刻度尺和所有轨道
    /// 时间轴区域比节点宽很多，提供充足的编辑空间
    /// </summary>
    public class TimelineView : VisualElement
    {
        // 配置常量
        public const float RULER_HEIGHT = 30f;  // 增加刻度栏高度
        public const float TRACK_HEIGHT = 24f;
        public const float TRACK_LABEL_WIDTH = 120f;  // 增加标签宽度以容纳"Cue1:特效节点"这样的命名
        public const float DEFAULT_PIXELS_PER_FRAME = 10f; // 默认每帧10像素
        public const float MIN_PIXELS_PER_FRAME = 2f;      // 最小缩放
        public const float MAX_PIXELS_PER_FRAME = 100f;    // 最大缩放
        public const int MIN_DURATION = 1;                  // 最小1帧
        public const float PORT_AREA_WIDTH = 24f;
        public const float TIMELINE_CONTENT_WIDTH =980f;  // 增加时间轴内容区域宽度，让输出点紧贴右侧

        // 状态（使用帧作为单位）
        public int DurationFrames { get; private set; } = 10; // 默认10帧
        public float PixelsPerFrame { get; private set; } = DEFAULT_PIXELS_PER_FRAME;

        // UI元素
        private VisualElement _rulerRow;
        private TimelineRuler _ruler;
        private VisualElement _trackContainer;
        private List<TimelineTrackBase> _tracks = new List<TimelineTrackBase>();
        private VisualElement _endMarkerLine; // 红色结束标记线
        private PlaybackIndicator _playbackIndicator; // 绿色播放指示器

        // 数据引用
        private AnimationNodeData _data;
        private Func<Port> _createPortFunc;

        // 事件
        public event Action OnDataChanged;
        public event Action OnAddButtonClicked;

        public TimelineView()
        {
            style.flexDirection = FlexDirection.Column;
            style.marginTop = 4;

            // 创建刻度尺行
            _rulerRow = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    height = RULER_HEIGHT,
                    overflow = Overflow.Hidden // 防止刻度尺溢出
                }
            };

            // 标题 "时间轴" 文字
            var titleLabel = new Label("时间轴")
            {
                style =
                {
                    width = TRACK_LABEL_WIDTH,
                    fontSize = 11,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    color = Color.white,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingLeft = 8,  // 增加左侧padding
                    backgroundColor = new Color(45f / 255f, 45f / 255f, 45f / 255f)
                }
            };
            _rulerRow.Add(titleLabel);

            // 创建刻度尺
            _ruler = new TimelineRuler();
            _ruler.style.height = RULER_HEIGHT;
            _ruler.style.flexGrow = 0; // 使用固定宽度，不自动扩展
            _ruler.style.flexShrink = 0; // 不收缩
            _ruler.OnPixelsPerFrameChanged += OnRulerPixelsPerFrameChanged;

            // 添加鼠标滚轮缩放功能
            _ruler.RegisterCallback<WheelEvent>(OnRulerWheel);

            _rulerRow.Add(_ruler);

            // 添加按钮（在刻度尺右侧）
            var addButton = new Button { text = "+" };
            addButton.style.width = PORT_AREA_WIDTH;
            addButton.style.height = RULER_HEIGHT;
            addButton.style.paddingLeft = 0;
            addButton.style.paddingRight = 0;
            addButton.style.paddingTop = 0;
            addButton.style.paddingBottom = 0;
            addButton.style.fontSize = 14;
            addButton.style.backgroundColor = new Color(45f / 255f, 45f / 255f, 45f / 255f);
            addButton.clicked += () => OnAddButtonClicked?.Invoke();
            _rulerRow.Add(addButton);

            Add(_rulerRow);

            // 创建轨道容器
            _trackContainer = new VisualElement
            {
                name = "TrackContainer",
                style =
                {
                    flexDirection = FlexDirection.Column,
                    overflow = Overflow.Hidden // 防止轨道溢出
                }
            };
            Add(_trackContainer);

            // 创建红色结束标记线
            _endMarkerLine = new VisualElement
            {
                name = "EndMarkerLine",
                style =
                {
                    position = Position.Absolute,
                    width = 2,
                    backgroundColor = new Color(1f, 0.2f, 0.2f), // 红色
                    top = RULER_HEIGHT,
                    bottom = 0
                }
            };
            Add(_endMarkerLine);

            // 创建绿色播放指示器
            _playbackIndicator = new PlaybackIndicator(this);
            Add(_playbackIndicator);
        }

        /// <summary>
        /// 鼠标滚轮缩放时间刻度
        /// </summary>
        private void OnRulerWheel(WheelEvent evt)
        {
            // 向上滚动放大，向下滚动缩小
            float delta = -evt.delta.y;
            float zoomFactor = delta > 0 ? 1.1f : 0.9f;

            float newPixelsPerFrame = PixelsPerFrame * zoomFactor;
            newPixelsPerFrame = Mathf.Clamp(newPixelsPerFrame, MIN_PIXELS_PER_FRAME, MAX_PIXELS_PER_FRAME);

            if (Mathf.Abs(newPixelsPerFrame - PixelsPerFrame) > 0.01f)
            {
                PixelsPerFrame = newPixelsPerFrame;

                // 只更新刻度尺的刻度，不改变宽度
                _ruler.SetDuration(DurationFrames, PixelsPerFrame);

                // 只更新所有轨道的内容位置，不改变宽度
                foreach (var track in _tracks)
                {
                    track.UpdateLayout();
                }

                // 更新结束标记线位置
                UpdateEndMarkerLine();

                // 更新播放指示器位置
                if (_playbackIndicator != null)
                    _playbackIndicator.CurrentFrame = _playbackIndicator.CurrentFrame;
            }

            evt.StopPropagation();
        }

        /// <summary>
        /// 当刻度尺的PixelsPerFrame变化时更新
        /// </summary>
        private void OnRulerPixelsPerFrameChanged(float newPixelsPerFrame)
        {
            PixelsPerFrame = newPixelsPerFrame;

            // 只更新所有轨道的内容位置，不改变宽度
            foreach (var track in _tracks)
            {
                track.UpdateLayout();
            }

            // 更新结束标记线位置
            UpdateEndMarkerLine();

            // 更新播放指示器位置
            if (_playbackIndicator != null)
                _playbackIndicator.CurrentFrame = _playbackIndicator.CurrentFrame;
        }

        /// <summary>
        /// 更新结束标记线位置
        /// </summary>
        private void UpdateEndMarkerLine()
        {
            if (_endMarkerLine == null) return;

            float endX = FrameToPosition(DurationFrames);
            float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;

            // 如果红线超出可见区域，隐藏它
            if (endX > availableWidth)
            {
                _endMarkerLine.style.display = DisplayStyle.None;
            }
            else
            {
                _endMarkerLine.style.display = DisplayStyle.Flex;
                _endMarkerLine.style.left = TRACK_LABEL_WIDTH + endX;
            }
        }

        /// <summary>
        /// 初始化Timeline视图
        /// </summary>
        public void Initialize(AnimationNodeData data, Func<Port> createPortFunc)
        {
            _data = data;
            _createPortFunc = createPortFunc;

            // 解析动画时长
            UpdateDuration();

            // 计算初始的pixelsPerFrame以填满可用宽度
            float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
            if (DurationFrames > 0)
            {
                PixelsPerFrame = availableWidth / DurationFrames;
                PixelsPerFrame = Mathf.Clamp(PixelsPerFrame, MIN_PIXELS_PER_FRAME, MAX_PIXELS_PER_FRAME);
            }

            // 设置ruler和tracks的固定宽度
            _ruler.SetFixedWidth(availableWidth);
            _ruler.SetDuration(DurationFrames, PixelsPerFrame);

            // 刷新所有轨道
            RefreshAllTracks();
        }

        /// <summary>
        /// 更新时长（从动画配置解析）
        /// </summary>
        public void UpdateDuration()
        {
            if (_data == null) return;

            // 尝试解析动画时长（帧数）
            if (int.TryParse(_data.animationDuration, out int frames))
            {
                DurationFrames = Mathf.Max(frames, MIN_DURATION);
            }
            else
            {
                DurationFrames = 10; // 默认10帧
            }

            // 重新计算pixelsPerFrame以填满可用宽度
            float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
            if (DurationFrames > 0)
            {
                PixelsPerFrame = availableWidth / DurationFrames;
                PixelsPerFrame = Mathf.Clamp(PixelsPerFrame, MIN_PIXELS_PER_FRAME, MAX_PIXELS_PER_FRAME);
            }

            // 更新刻度尺（只更新刻度，不改变宽度）
            _ruler.SetDuration(DurationFrames, PixelsPerFrame);

            // 更新所有轨道布局（只更新内容位置，不改变宽度）
            foreach (var track in _tracks)
            {
                track.UpdateLayout();
            }

            // 更新结束标记线位置
            UpdateEndMarkerLine();
        }

        /// <summary>
        /// 设置时长
        /// </summary>
        public void SetDuration(int frames)
        {
            DurationFrames = Mathf.Max(frames, MIN_DURATION);

            // 裁剪超出范围的时间点
            ClampAllTimesToDuration();

            // 更新刻度尺（只更新刻度，不改变宽度）
            _ruler.SetDuration(DurationFrames, PixelsPerFrame);

            // 更新所有轨道布局（只更新内容位置，不改变宽度）
            foreach (var track in _tracks)
            {
                track.UpdateLayout();
            }

            // 更新结束标记线位置
            UpdateEndMarkerLine();
        }

        /// <summary>
        /// 裁剪所有时间点到有效范围
        /// </summary>
        private void ClampAllTimesToDuration()
        {
            if (_data == null) return;

            // 裁剪时间效果
            if (_data.timeEffects != null)
            {
                foreach (var effect in _data.timeEffects)
                {
                    effect.triggerTime = Mathf.Clamp(effect.triggerTime, 0, (int)DurationFrames);
                }
            }

            // 裁剪时间Cue（只限制startTime下限，不限制上限）
            if (_data.timeCues != null)
            {
                foreach (var cue in _data.timeCues)
                {
                    cue.startTime = Mathf.Max(cue.startTime, 0);
                    if (cue.endTime >= 0)
                    {
                        cue.endTime = Mathf.Max(cue.endTime, cue.startTime + 1);
                    }
                }
            }
        }

        /// <summary>
        /// 刷新所有轨道（智能刷新，保留已有轨道的连接）
        /// </summary>
        public void RefreshAllTracks()
        {
            if (_data == null) return;

            // 计算需要的轨道数量
            int cueCount = _data.timeCues?.Count ?? 0;
            int effectCount = _data.timeEffects?.Count ?? 0;
            int totalNeeded = cueCount + effectCount;

            // 如果轨道数量匹配，只需要更新布局，不重新创建
            if (_tracks.Count == totalNeeded)
            {
                // 更新所有轨道的布局
                foreach (var track in _tracks)
                {
                    track.UpdateLayout();
                }
                return;
            }

            // 轨道数量不匹配，需要重新创建
            // 清除旧轨道
            foreach (var track in _tracks)
            {
                track.RemoveFromHierarchy();
            }
            _tracks.Clear();
            _trackContainer.Clear();

            // 先添加时间Cue轨道（显示在上面）
            if (_data.timeCues != null)
            {
                for (int i = 0; i < _data.timeCues.Count; i++)
                {
                    AddTimeCueTrack(_data.timeCues[i], i);
                }
            }

            // 再添加时间效果轨道（显示在下面）
            if (_data.timeEffects != null)
            {
                for (int i = 0; i < _data.timeEffects.Count; i++)
                {
                    AddTimeEffectTrack(_data.timeEffects[i], i);
                }
            }
        }

        /// <summary>
        /// 添加单个新轨道（不影响已有轨道）
        /// </summary>
        public void AddNewTrack(bool isCue)
        {
            if (_data == null) return;

            if (isCue)
            {
                // 添加新的Cue轨道
                int index = (_data.timeCues?.Count ?? 1) - 1;
                var data = _data.timeCues[index];
                var track = new TimeCueTrack(this, data, index, _createPortFunc);
                track.OnRemoveRequested += () => RemoveTimeCue(index);
                track.OnDataModified += NotifyDataChanged;

                // 设置轨道内容区域的固定宽度
                float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
                track.UpdateTrackContentWidth(availableWidth);

                // Cue轨道插入到最前面（在所有效果轨道之前）
                int insertIndex = 0;
                for (int i = 0; i < _tracks.Count; i++)
                {
                    if (_tracks[i] is TimeEffectTrack)
                    {
                        insertIndex = i;
                        break;
                    }
                    insertIndex = i + 1;
                }

                _trackContainer.Insert(insertIndex, track);
                _tracks.Insert(insertIndex, track);
            }
            else
            {
                // 添加新的效果轨道
                int index = (_data.timeEffects?.Count ?? 1) - 1;
                var data = _data.timeEffects[index];
                var track = new TimeEffectTrack(this, data, index, _createPortFunc);
                track.OnRemoveRequested += () => RemoveTimeEffect(index);
                track.OnDataModified += NotifyDataChanged;

                // 设置轨道内容区域的固定宽度
                float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
                track.UpdateTrackContentWidth(availableWidth);

                // 效果轨道添加到最后
                _trackContainer.Add(track);
                _tracks.Add(track);
            }
        }

        /// <summary>
        /// 添加时间效果轨道
        /// </summary>
        private void AddTimeEffectTrack(TimeEffectData data, int index)
        {
            var track = new TimeEffectTrack(this, data, index, _createPortFunc);
            track.OnRemoveRequested += () => RemoveTimeEffect(index);
            track.OnDataModified += NotifyDataChanged;

            // 设置轨道内容区域的固定宽度
            float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
            track.UpdateTrackContentWidth(availableWidth);

            _trackContainer.Add(track);
            _tracks.Add(track);
        }

        /// <summary>
        /// 添加时间Cue轨道
        /// </summary>
        private void AddTimeCueTrack(TimeCueData data, int index)
        {
            var track = new TimeCueTrack(this, data, index, _createPortFunc);
            track.OnRemoveRequested += () => RemoveTimeCue(index);
            track.OnDataModified += NotifyDataChanged;

            // 设置轨道内容区域的固定宽度
            float availableWidth = TIMELINE_CONTENT_WIDTH - TRACK_LABEL_WIDTH - PORT_AREA_WIDTH;
            track.UpdateTrackContentWidth(availableWidth);

            _trackContainer.Add(track);
            _tracks.Add(track);
        }

        /// <summary>
        /// 删除时间效果
        /// </summary>
        private void RemoveTimeEffect(int index)
        {
            if (_data == null || _data.timeEffects == null) return;
            if (index < 0 || index >= _data.timeEffects.Count) return;

            _data.timeEffects.RemoveAt(index);
            NotifyDataChanged();
            RefreshAllTracks();
        }

        /// <summary>
        /// 删除时间Cue
        /// </summary>
        private void RemoveTimeCue(int index)
        {
            if (_data == null || _data.timeCues == null) return;
            if (index < 0 || index >= _data.timeCues.Count) return;

            _data.timeCues.RemoveAt(index);
            NotifyDataChanged();
            RefreshAllTracks();
        }

        /// <summary>
        /// 帧转换为位置
        /// </summary>
        public float FrameToPosition(float frame)
        {
            return frame * PixelsPerFrame;
        }

        /// <summary>
        /// 位置转换为帧
        /// </summary>
        public float PositionToFrame(float x)
        {
            return x / PixelsPerFrame;
        }

        /// <summary>
        /// 时间转换为位置（兼容旧接口）
        /// </summary>
        public float TimeToPosition(float time)
        {
            return time * SkillEditorConstants.DEFAULT_FPS * PixelsPerFrame;
        }

        /// <summary>
        /// 位置转换为时间（兼容旧接口）
        /// </summary>
        public float PositionToTime(float x)
        {
            return x / (SkillEditorConstants.DEFAULT_FPS * PixelsPerFrame);
        }

        /// <summary>
        /// 根据端口标识符查找端口
        /// </summary>
        public Port FindPortByIdentifier(string portIdentifier)
        {
            foreach (var track in _tracks)
            {
                if (track.PortId == portIdentifier)
                {
                    return track.GetOutputPort();
                }
            }
            return null;
        }

        /// <summary>
        /// 获取所有输出端口
        /// </summary>
        public List<Port> GetAllOutputPorts()
        {
            var ports = new List<Port>();
            foreach (var track in _tracks)
            {
                var port = track.GetOutputPort();
                if (port != null)
                {
                    ports.Add(port);
                }
            }
            return ports;
        }

        /// <summary>
        /// 通知数据变化
        /// </summary>
        public void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }

        #region 播放指示器

        /// <summary>
        /// 获取播放指示器（供AnimationNode绑定事件）
        /// </summary>
        public PlaybackIndicator GetPlaybackIndicator() => _playbackIndicator;

        /// <summary>
        /// 设置播放指示器当前帧
        /// </summary>
        public void SetPlaybackFrame(int frame)
        {
            if (_playbackIndicator != null)
                _playbackIndicator.CurrentFrame = frame;
        }

        /// <summary>
        /// 设置播放指示器可见性
        /// </summary>
        public void SetPlaybackIndicatorVisible(bool visible)
        {
            if (_playbackIndicator != null)
                _playbackIndicator.SetVisible(visible);
        }

        #endregion
    }
}
