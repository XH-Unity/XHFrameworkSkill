using UnityEngine;
using UnityEditor.Experimental.GraphView;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 表现节点基类 - 所有Cue节点继承此类
    /// 对应GAS的GameplayCue，用于播放视觉/音频表现
    /// 表现节点通常不需要输出端口（纯表现，不继续流程）
    /// </summary>
    public abstract class CueNode<TData> : SkillNodeBase<TData> where TData : CueNodeData, new()
    {
        protected CueNode(NodeType nodeType, Vector2 position) : base(nodeType, position)
        {
        }

        /// <summary>
        /// 表现节点默认有输入端口
        /// </summary>
        protected override bool HasDefaultInputPort => true;

        /// <summary>
        /// 通知连接的TimeCueTrack重新计算自动时长
        /// 当Cue节点的资源数据变化时（如prefab、AudioClip）调用
        /// </summary>
        public void NotifyConnectedTracksUpdateDuration()
        {
            if (defaultInputPort == null) return;

            foreach (var edge in defaultInputPort.connections)
            {
                // edge.output 是 TimeCueTrack 的 OutputPort
                // edge.output 的父级链中可以找到 TimeCueTrack
                var outputPort = edge?.output;
                if (outputPort == null) continue;

                // 向上查找 TimeCueTrack
                var element = outputPort.parent;
                while (element != null)
                {
                    if (element is TimeCueTrack track)
                    {
                        track.UpdateAutoDuration();
                        break;
                    }
                    element = element.parent;
                }
            }
        }
    }
}
