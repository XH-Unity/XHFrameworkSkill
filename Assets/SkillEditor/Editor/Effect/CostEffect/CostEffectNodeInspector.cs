using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 消耗效果节点Inspector
    /// 显示消耗属性修改器列表
    /// </summary>
    public class CostEffectNodeInspector : EffectNodeInspector
    {
        // 消耗不需要显示节点目标
        protected override bool ShowTargetType => false;

        protected override bool ShowAttributeModifiers => true;
        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var costData = node?.NodeData as CostEffectNodeData;
            if (costData == null) return;
        }
    }
}
