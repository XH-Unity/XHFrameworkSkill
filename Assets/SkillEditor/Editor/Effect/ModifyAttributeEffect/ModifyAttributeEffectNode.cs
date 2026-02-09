using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 属性修改效果节点（瞬时效果）
    /// 使用基类的 attributeModifiers 配置属性修改
    /// </summary>
    public class ModifyAttributeEffectNode : EffectNode<ModifyAttributeEffectNodeData>
    {
        // 输出端口
        private Port completePort;

        public ModifyAttributeEffectNode(Vector2 position) : base(NodeType.ModifyAttributeEffect, position) { }

        protected override string GetNodeTitle() => "属性修改";
        protected override float GetNodeWidth() => 250;

        // 显示属性修改器配置
        protected override bool ShowAttributeModifiers => true;

        protected override void CreateEffectContent()
        {
        }
    }
}
