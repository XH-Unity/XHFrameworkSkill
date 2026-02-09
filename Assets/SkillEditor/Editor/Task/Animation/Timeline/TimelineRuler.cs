using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 时间刻度尺 - 显示时间标记和刻度线（使用帧作为单位）
    /// </summary>
    public class TimelineRuler : VisualElement
    {
        private int _durationFrames = 10;
        private float _pixelsPerFrame = TimelineView.DEFAULT_PIXELS_PER_FRAME;

        // 刻度配置 - 动态计算
        private const float MAJOR_TICK_HEIGHT = 12f;   // 主刻度高度
        private const float MINOR_TICK_HEIGHT = 6f;    // 次刻度高度
        private const float MIN_TICK_SPACING = 50f;    // 最小刻度间距（像素）

        // 颜色
        private static readonly Color TICK_COLOR = new Color(0.5f, 0.5f, 0.5f);
        private static readonly Color TEXT_COLOR = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color BACKGROUND_COLOR = new Color(45f / 255f, 45f / 255f, 45f / 255f);

        // 事件：当pixelsPerFrame变化时通知
        public event Action<float> OnPixelsPerFrameChanged;

        public TimelineRuler()
        {
            style.backgroundColor = BACKGROUND_COLOR;
            style.overflow = Overflow.Hidden;
            generateVisualContent += OnGenerateVisualContent;
        }

        /// <summary>
        /// 设置固定宽度（背景宽度，不随缩放变化）
        /// </summary>
        public void SetFixedWidth(float width)
        {
            style.width = width;
        }

        /// <summary>
        /// 设置时长和缩放
        /// </summary>
        public void SetDuration(int durationFrames, float pixelsPerFrame)
        {
            _durationFrames = durationFrames;
            _pixelsPerFrame = pixelsPerFrame;

            // 不改变宽度，只更新刻度和标签

            // 重新绘制
            MarkDirtyRepaint();

            // 更新时间标签
            UpdateTimeLabels();
        }

        /// <summary>
        /// 获取刻度尺需要的宽度
        /// </summary>
        public float GetRequiredWidth()
        {
            return _durationFrames * _pixelsPerFrame;
        }

        /// <summary>
        /// 计算动态刻度间隔（像Unity Timeline）
        /// </summary>
        private void GetTickIntervals(out int majorInterval, out int minorInterval)
        {
            // 计算主刻度间隔，使得刻度之间的像素距离不小于MIN_TICK_SPACING
            // 可能的刻度间隔（帧数）：1, 2, 5, 10, 20, 50, 100, 200, 500, 1000...
            int[] intervals = { 1, 2, 5, 10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10000 };

            majorInterval = 1; // 默认最小为1帧

            foreach (int interval in intervals)
            {
                float spacing = interval * _pixelsPerFrame;
                if (spacing >= MIN_TICK_SPACING)  // 至少50像素间距
                {
                    majorInterval = interval;
                    break;
                }
            }

            // 确保主刻度间隔至少为1帧
            majorInterval = Mathf.Max(1, majorInterval);

            // 次刻度间隔是主刻度的1/5或1/2
            if (majorInterval >= 10)
                minorInterval = majorInterval / 5;
            else if (majorInterval >= 2)
                minorInterval = majorInterval / 2;
            else
                minorInterval = 1; // 最小为1帧
        }

        /// <summary>
        /// 绘制刻度尺
        /// </summary>
        private void OnGenerateVisualContent(MeshGenerationContext ctx)
        {
            var painter = ctx.painter2D;
            float height = resolvedStyle.height;
            float width = resolvedStyle.width;

            // 获取动态刻度间隔
            GetTickIntervals(out int majorInterval, out int minorInterval);

            // 绘制次刻度
            painter.strokeColor = TICK_COLOR;
            painter.lineWidth = 1f;

            // 计算可见范围内的最大帧数（填满整个可见区域，不限制在动画长度内）
            int maxVisibleFrame = Mathf.CeilToInt(width / _pixelsPerFrame) + majorInterval;

            for (int frame = 0; frame <= maxVisibleFrame; frame += minorInterval)
            {
                // 跳过主刻度位置（会单独绘制）
                if (frame % majorInterval == 0)
                    continue;

                float x = frame * _pixelsPerFrame;
                if (x > width) break;

                painter.BeginPath();
                painter.MoveTo(new Vector2(x, height - MINOR_TICK_HEIGHT));
                painter.LineTo(new Vector2(x, height));
                painter.Stroke();
            }

            // 绘制主刻度
            painter.lineWidth = 1.5f;

            for (int frame = 0; frame <= maxVisibleFrame; frame += majorInterval)
            {
                float x = frame * _pixelsPerFrame;
                if (x > width) break;

                painter.BeginPath();
                painter.MoveTo(new Vector2(x, height - MAJOR_TICK_HEIGHT));
                painter.LineTo(new Vector2(x, height));
                painter.Stroke();
            }
        }

        /// <summary>
        /// 更新时间标签
        /// </summary>
        private void UpdateTimeLabels()
        {
            // 清除旧标签
            var oldLabels = this.Query<Label>().ToList();
            foreach (var label in oldLabels)
            {
                label.RemoveFromHierarchy();
            }

            // 获取动态刻度间隔
            GetTickIntervals(out int majorInterval, out int minorInterval);

            float width = resolvedStyle.width;
            if (width <= 0 || float.IsNaN(width))
            {
                width = 676f; // 使用默认宽度
            }

            // 计算可见范围内的最大帧数（填满整个可见区域，不限制在动画长度内）
            int maxVisibleFrame = Mathf.CeilToInt(width / _pixelsPerFrame) + majorInterval;

            // 创建新标签（显示帧数）
            for (int frame = 0; frame <= maxVisibleFrame; frame += majorInterval)
            {
                float x = frame * _pixelsPerFrame;
                if (x > width) break;

                var label = new Label(frame.ToString())
                {
                    style =
                    {
                        position = Position.Absolute,
                        left = x + 2,
                        top = 2,
                        fontSize = 10,
                        color = TEXT_COLOR,
                        unityTextAlign = TextAnchor.UpperLeft,
                        unityFontStyleAndWeight = FontStyle.Normal
                    }
                };
                Add(label);
            }
        }
    }
}
