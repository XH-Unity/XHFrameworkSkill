using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 特效Cue节点 - 简洁版，只显示端口
    /// 所有属性在Inspector面板绘制
    /// </summary>
    public class ParticleCueNode : CueNode<ParticleCueNodeData>
    {
        public ParticleCueNode(Vector2 position) : base(NodeType.ParticleCue, position)
        {
        }

        protected override string GetNodeTitle() => "特效";
        protected override float GetNodeWidth() => 120;

        protected override void CreateContent()
        {
            // 节点上不显示内容，所有属性在Inspector面板绘制
        }

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
        }

        public override void SyncUIFromData()
        {
            // 节点上没有UI控件需要同步
        }
    }
}
