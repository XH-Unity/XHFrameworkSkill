using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Timeline轨道基类
    /// </summary>
    public abstract class TimelineTrackBase : VisualElement
    {
        public TimelineView Timeline { get; protected set; }  // 改为public
        protected Port OutputPort;
        protected VisualElement TrackLabel;
        protected VisualElement TrackContent;
        protected VisualElement PortArea;

        public string PortId { get; protected set; }

        // 事件
        public event Action OnRemoveRequested;
        public event Action OnDataModified;

        protected TimelineTrackBase(TimelineView timeline)
        {
            Timeline = timeline;

            style.flexDirection = FlexDirection.Row;
            style.height = TimelineView.TRACK_HEIGHT;
            style.alignItems = Align.Center;

            // 创建轨道标签区域（固定宽度）
            TrackLabel = new VisualElement
            {
                style =
                {
                    width = TimelineView.TRACK_LABEL_WIDTH,
                    height = TimelineView.TRACK_HEIGHT,
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    justifyContent = Justify.FlexStart,
                    paddingLeft = 4,
                    paddingRight = 10,  // 增加右侧padding，与轨道内容有更多间距
                    backgroundColor = new Color(50f / 255f, 50f / 255f, 50f / 255f),
                    borderRightWidth = 1,
                    borderRightColor = new Color(30f / 255f, 30f / 255f, 30f / 255f)
                }
            };
            Add(TrackLabel);

            // 创建轨道内容区域（时间轴区域，使用固定宽度，不随缩放变化）
            TrackContent = new VisualElement
            {
                style =
                {
                    flexGrow = 0,
                    flexShrink = 0,
                    height = TimelineView.TRACK_HEIGHT,
                    backgroundColor = new Color(40f / 255f, 40f / 255f, 40f / 255f),
                    borderBottomWidth = 1,
                    borderBottomColor = new Color(30f / 255f, 30f / 255f, 30f / 255f),
                    overflow = Overflow.Hidden // 超出部分隐藏
                }
            };
            Add(TrackContent);

            // 创建端口区域（固定宽度，放在最右侧）
            PortArea = new VisualElement
            {
                style =
                {
                    width = TimelineView.PORT_AREA_WIDTH,
                    height = TimelineView.TRACK_HEIGHT,
                    alignItems = Align.Center,
                    justifyContent = Justify.Center,
                    backgroundColor = new Color(50f / 255f, 50f / 255f, 50f / 255f),
                    borderLeftWidth = 1,
                    borderLeftColor = new Color(30f / 255f, 30f / 255f, 30f / 255f)
                }
            };
            Add(PortArea);
        }

        /// <summary>
        /// 更新轨道布局
        /// </summary>
        public abstract void UpdateLayout();

        /// <summary>
        /// 更新轨道内容区域的宽度
        /// </summary>
        public void UpdateTrackContentWidth(float width)
        {
            if (TrackContent != null)
            {
                TrackContent.style.width = width;
            }
        }

        /// <summary>
        /// 获取输出端口
        /// </summary>
        public Port GetOutputPort() => OutputPort;

        /// <summary>
        /// 触发删除请求
        /// </summary>
        protected void RequestRemove()
        {
            OnRemoveRequested?.Invoke();
        }

        /// <summary>
        /// 通知数据修改
        /// </summary>
        protected void NotifyDataModified()
        {
            OnDataModified?.Invoke();
        }
    }
}
