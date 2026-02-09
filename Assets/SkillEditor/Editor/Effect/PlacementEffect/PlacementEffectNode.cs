using UnityEditor.Experimental.GraphView;
using UnityEngine;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 放置物效果节点
    /// 支持进入/离开/停留三种事件端口
    /// </summary>
    public class PlacementEffectNode : EffectNode<PlacementEffectNodeData>
    {
        // 碰撞相关输出端口
        private Port onEnterPort;
        private Port onExitPort;

        public PlacementEffectNode(Vector2 position) : base(NodeType.PlacementEffect, position) { }

        protected override string GetNodeTitle() => "放置物";
        protected override float GetNodeWidth() => 160;

        // 显示完成效果端口
        protected override bool ShowCompleteEffectPort => true;

        // 不显示基类的周期端口，我们用自己的停留端口
        protected override bool ShowPeriodicEffectPort => true;

        // 显示持续时间配置
        protected override bool ShowDurationConfig => true;

        protected override void CreateEffectContent()
        {
            
            
            onEnterPort = CreateOutputPort("进入时");
            onEnterPort.portColor = new Color(0.4f, 0.8f, 0.4f); // 绿色
            
            onExitPort = CreateOutputPort("离开时");
            onExitPort.portColor = new Color(0.8f, 0.4f, 0.4f); // 红色
        }

     
   
    }
}
