using UnityEngine;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 通用效果节点 - 暴露基类全部配置能力
    /// </summary>
    public class GenericEffectNode : EffectNode<GenericEffectNodeData>
    {
        public GenericEffectNode(Vector2 position) : base(NodeType.GenericEffect, position) { }

        protected override string GetNodeTitle() => "通用效果";
        protected override float GetNodeWidth() => 200;

        // 显示全部配置
        protected override bool ShowDurationTypeConfig => true;
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => true;
        protected override bool ShowAttributeModifiers => true;
        protected override bool ShowStackConfig => true;

        // 显示全部输出端口
        protected override bool ShowInitialEffectPort => true;
        protected override bool ShowPeriodicEffectPort => true;
        protected override bool ShowCompleteEffectPort => true;

        protected override void CreateEffectContent()
        {
        }
    }
}
