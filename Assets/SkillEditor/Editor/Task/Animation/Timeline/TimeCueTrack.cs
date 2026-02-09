using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 时间Cue轨道 - 显示为矩形区域，支持整体拖拽和边缘调整
    /// </summary>
    public class TimeCueTrack : TimelineTrackBase
    {
        private TimeCueData _data;
        private int _index;
        private VisualElement _region;
        private VisualElement _leftHandle;
        private VisualElement _rightHandle;
        private Label _nameLabel;  // 保存标签引用以便更新

        // 配置
        private const float HANDLE_WIDTH = 8f;  // 手柄宽度
        private const float MIN_DURATION = 1f;  // 最小1帧
        private const float REGION_HEIGHT = 18f;

        // 颜色配置
        private static readonly Color REGION_COLOR = new Color(0.2f, 0.7f, 0.4f, 0.8f); // 绿色
        private static readonly Color REGION_HOVER_COLOR = new Color(0.3f, 0.8f, 0.5f, 0.9f);
        private static readonly Color HANDLE_COLOR = new Color(0.15f, 0.15f, 0.15f, 0.6f); // 深色半透明
        private static readonly Color HANDLE_HOVER_COLOR = new Color(1f, 1f, 1f, 0.9f); // 白色高亮

        public TimeCueTrack(TimelineView timeline, TimeCueData data, int index, Func<Port> createPortFunc)
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
            _nameLabel = new Label($"Cue {index + 1}")
            {
                style =
                {
                    fontSize = 10,
                    color = REGION_COLOR,
                    unityFontStyleAndWeight = FontStyle.Bold
                }
            };
            TrackLabel.Add(_nameLabel);

            // 创建区域
            CreateRegion();

            // 创建输出端口
            if (createPortFunc != null)
            {
                OutputPort = createPortFunc();
                OutputPort.portName = "";
                OutputPort.portColor = REGION_COLOR;
                OutputPort.name = data.portId;
                PortArea.Add(OutputPort);

                // 如果是TimelinePort，监听连接变化事件
                if (OutputPort is TimelinePort timelinePort)
                {
                    timelinePort.OnConnectionChanged += UpdateTrackName;
                    timelinePort.OnConnectionChanged += UpdateAutoDuration;
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

            string baseName = $"Cue {_index + 1}";

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
        /// 创建时间区域
        /// </summary>
        private void CreateRegion()
        {
            _region = new VisualElement
            {
                name = "TimeCueRegion",
                style =
                {
                    position = Position.Absolute,
                    height = REGION_HEIGHT,
                    top = (TimelineView.TRACK_HEIGHT - REGION_HEIGHT) / 2,
                    backgroundColor = REGION_COLOR,
                    borderTopLeftRadius = 3,
                    borderTopRightRadius = 3,
                    borderBottomLeftRadius = 3,
                    borderBottomRightRadius = 3,
                    justifyContent = Justify.Center,
                    alignItems = Align.Center
                }
            };

            // 添加文本标签显示时间信息
            var timeLabel = new Label
            {
                name = "TimeLabel",
                pickingMode = PickingMode.Ignore, // 不拦截鼠标事件，让事件穿透到下层region
                style =
                {
                    fontSize = 8,
                    color = Color.white,
                    unityFontStyleAndWeight = FontStyle.Bold,
                    unityTextAlign = TextAnchor.MiddleCenter
                }
            };
            _region.Add(timeLabel);

            // 左边缘手柄（用于调整开始时间）
            _leftHandle = new VisualElement
            {
                name = "LeftHandle",
                style =
                {
                    position = Position.Absolute,
                    left = 0,
                    top = 0,
                    width = HANDLE_WIDTH,
                    height = REGION_HEIGHT,
                    backgroundColor = HANDLE_COLOR, // 默认显示深色半透明
                    borderTopLeftRadius = 3,
                    borderBottomLeftRadius = 3
                }
            };
            _leftHandle.RegisterCallback<MouseEnterEvent>(evt =>
            {
                _leftHandle.style.backgroundColor = HANDLE_HOVER_COLOR; // 悬停时白色高亮
            });
            _leftHandle.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _leftHandle.style.backgroundColor = HANDLE_COLOR; // 恢复深色
            });
            _region.Add(_leftHandle);

            // 右边缘手柄（用于调整结束时间）
            _rightHandle = new VisualElement
            {
                name = "RightHandle",
                style =
                {
                    position = Position.Absolute,
                    right = 0,
                    top = 0,
                    width = HANDLE_WIDTH,
                    height = REGION_HEIGHT,
                    backgroundColor = HANDLE_COLOR, // 默认显示深色半透明
                    borderTopRightRadius = 3,
                    borderBottomRightRadius = 3
                }
            };
            _rightHandle.RegisterCallback<MouseEnterEvent>(evt =>
            {
                _rightHandle.style.backgroundColor = HANDLE_HOVER_COLOR; // 悬停时白色高亮
            });
            _rightHandle.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _rightHandle.style.backgroundColor = HANDLE_COLOR; // 恢复深色
            });
            _region.Add(_rightHandle);

            // 区域悬停效果
            _region.RegisterCallback<MouseEnterEvent>(evt =>
            {
                _region.style.backgroundColor = REGION_HOVER_COLOR;
            });
            _region.RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _region.style.backgroundColor = REGION_COLOR;
            });

            // 使用Manipulator处理拖拽（直接在手柄和区域上注册事件）
            var manipulator = new CueRegionManipulator(this, _region, _leftHandle, _rightHandle);

            TrackContent.Add(_region);
        }

        /// <summary>
        /// 更新布局
        /// </summary>
        public override void UpdateLayout()
        {
            if (_region == null) return;

            float startX = Timeline.FrameToPosition(_data.startTime);

            // 计算结束位置
            int endFrame = _data.endTime;
            if (endFrame < 0)
            {
                // -1 表示动画结束时自动结束
                endFrame = (int)Timeline.DurationFrames;
            }
            float endX = Timeline.FrameToPosition(endFrame);

            float width = Mathf.Max(endX - startX, HANDLE_WIDTH * 2 + 4);

            _region.style.left = startX;
            _region.style.width = width;

            // 更新时间标签
            var timeLabel = _region.Q<Label>("TimeLabel");
            if (timeLabel != null)
            {
                timeLabel.text = $"{_data.startTime} - {endFrame}";
                // 如果宽度太小，隐藏文本
                timeLabel.style.display = width > 60 ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        #region 公共方法供Manipulator调用

        public int GetStartTime() => _data.startTime;
        public int GetEndTime() => _data.endTime < 0 ? (int)Timeline.DurationFrames : _data.endTime;

        public void MoveRegion(float originalStart, float originalEnd, float deltaFrame)
        {
            float duration = originalEnd - originalStart;
            int newStartFrame = (int)Mathf.Max(originalStart + deltaFrame, 0);
            _data.startTime = newStartFrame;

            // 如果原来是-1（自动结束），保持-1
            if (_data.endTime >= 0)
            {
                _data.endTime = (int)(newStartFrame + duration);
            }

            UpdateLayout();
        }

        public void ResizeLeft(float originalStart, float originalEnd, float deltaFrame)
        {
            float maxStart = originalEnd - MIN_DURATION;
            _data.startTime = (int)Mathf.Clamp(originalStart + deltaFrame, 0, maxStart);
            UpdateLayout();
        }

        public void ResizeRight(float originalStart, float originalEnd, float deltaFrame)
        {
            int newEndFrame = (int)Mathf.Max(originalEnd + deltaFrame, originalStart + MIN_DURATION);
            _data.endTime = newEndFrame;
            UpdateLayout();
        }

        public void NotifyDataChanged()
        {
            NotifyDataModified();
        }

        #endregion

        #region 自动时长

        /// <summary>
        /// 根据连接的Cue节点资源自动设置区域时长
        /// </summary>
        public void UpdateAutoDuration()
        {
            if (OutputPort == null || !OutputPort.connected) return;

            foreach (var edge in OutputPort.connections)
            {
                if (edge?.input?.node is SkillNodeBase skillNode)
                {
                    var nodeData = skillNode.NodeData;
                    int durationFrames = CueDurationHelper.GetCueDurationFrames(nodeData);
                    if (durationFrames > 0)
                    {
                        // 设置endTime = startTime + 资源时长，不限制于Timeline总时长
                        int newEnd = _data.startTime + durationFrames;
                        _data.endTime = newEnd;
                        UpdateLayout();
                        NotifyDataModified();
                        return;
                    }
                }
            }
        }

        #endregion
    }
}
