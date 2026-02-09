using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// Buff效果节点Inspector
    /// </summary>
    public class BuffEffectNodeInspector : EffectNodeInspector
    {
        // 显示所有Buff相关配置
        protected override bool ShowDurationConfig => true;
        protected override bool ShowPeriodicConfig => true;
        protected override bool ShowAttributeModifiers => true;
        protected override bool ShowStackConfig => true;

        protected override void BuildEffectInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var buffData = node?.NodeData as BuffEffectNodeData;
            if (buffData == null) return;

            // Buff基本信息
            var basicSection = CreateCollapsibleSection("Buff信息", out var basicContent, true);

            // Buff ID
            var idField = CreateIntField("Buff ID", buffData.buffId, value =>
            {
                buffData.buffId = value;
                node.SyncUIFromData();
            });
            basicContent.Add(idField);

            container.Add(basicSection);
        }

        protected override EffectNodeData GetEffectNodeData(SkillNodeBase node)
        {
            return node?.NodeData as BuffEffectNodeData;
        }
    }
}
