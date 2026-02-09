using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Buff效果节点（统一的Buff类型）
    /// 合并了原来的 DurationBuffEffectNode 和 PeriodicBuffEffectNode
    /// 通过 isPeriodic 属性区分是否为周期Buff
    /// </summary>
    public class BuffEffectNode : EffectNode<BuffEffectNodeData>
    {
        public BuffEffectNode(Vector2 position) : base(NodeType.BuffEffect, position) { }

        protected override string GetNodeTitle() => "Buff";
        protected override float GetNodeWidth() => 280;

        // 显示所有Buff相关配置
        protected override bool ShowDurationTypeConfig => true;
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => true;
        protected override bool ShowAttributeModifiers => true;
        protected override bool ShowStackConfig => true;

        // 显示所有生命周期端口
        protected override bool ShowInitialEffectPort => true;
        protected override bool ShowPeriodicEffectPort => true;
        protected override bool ShowRefreshEffectPort => true;
        protected override bool ShowCompleteEffectPort => true;
        protected override bool ShowRemoveAllEffectPort => true;
        protected override bool ShowOverflowEffectPort => true;  // 新增：溢出效果端口

        protected override void CreateEffectContent()
        {
            // Buff特有内容在基类的配置UI中已经处理
        }

        protected override void SyncEffectContentFromData()
        {
            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var buffId = TypedData?.buffId ?? 0;
            if (buffId <= 0)
            {
                title = TypedData?.isPeriodic == true ? "周期Buff" : "Buff";
            }
            else
            {
                title = TypedData?.isPeriodic == true ? $"周期Buff [{buffId}]" : $"Buff [{buffId}]";
            }
        }
    }
}
