using UnityEngine.UIElements;
using UnityEngine;
using System.Collections.Generic;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    /// <summary>
    /// 属性修改效果节点Inspector
    /// 显示属性修改器列表
    /// </summary>
    public class ModifyAttributeEffectNodeInspector : EffectNodeInspector
    {
        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var modifyData = node?.NodeData as ModifyAttributeEffectNodeData;
            if (modifyData == null) return;

            // 确保列表已初始化
            if (modifyData.attributeModifiers == null)
                modifyData.attributeModifiers = new List<AttributeModifierData>();

            // 创建属性修改配置区域
            var section = CreateCollapsibleSection("属性修改配置", out var content, true);
            RefreshModifiersList(content, node, modifyData.attributeModifiers, "+ 添加属性修改");
            container.Add(section);
        }
    }
}
