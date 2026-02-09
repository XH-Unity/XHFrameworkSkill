using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 消耗效果节点（瞬时效果）
    /// 使用基类的 attributeModifiers 配置消耗
    /// </summary>
    public class CostEffectNode : EffectNode<CostEffectNodeData>
    {
        public CostEffectNode(Vector2 position) : base(NodeType.CostEffect, position) { }

        protected override string GetNodeTitle() => "消耗";
        protected override float GetNodeWidth() => 250;

        // 显示属性修改器配置
        protected override bool ShowAttributeModifiers => true;

        protected override void CreateEffectContent()
        {
            // 消耗节点不需要输出端口
        }
    }
}
