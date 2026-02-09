using UnityEngine.UIElements;
using UnityEngine;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class AbilityNodeInspector : NodeInspectorBase
    {
        // 技能根节点不需要节点目标选择
        protected override bool ShowTargetType => false;

        protected override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is AbilityNode abilityNode)
            {
                var data = abilityNode.TypedData;
                if (data == null) return;

                container.Add(CreateIntField("技能ID", data.skillId, value => { data.skillId = value; abilityNode.SyncUIFromData(); }));
                container.Add(CreateTagsSection(abilityNode, data));
            }
        }

        private VisualElement CreateTagsSection(AbilityNode abilityNode, AbilityNodeData data)
        {
            var section = CreateCollapsibleSection("技能标签", out var content);

            content.Add(CreateTagSetField("技能标签", data.assetTags, value => { data.assetTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("激活时获得标签", data.activationOwnedTags, value => { data.activationOwnedTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("激活时所需标签", data.activationRequiredTags, value => { data.activationRequiredTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("激活时阻止标签", data.activationBlockedTags, value => { data.activationBlockedTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("运行时阻止标签", data.ongoingBlockedTags, value => { data.ongoingBlockedTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("激活时取消其他技能", data.cancelAbilitiesWithTags, value => { data.cancelAbilitiesWithTags = value; abilityNode.SyncUIFromData(); }));
            content.Add(CreateTagSetField("激活时阻塞其他激活", data.blockAbilitiesWithTags, value => { data.blockAbilitiesWithTags = value; abilityNode.SyncUIFromData(); }));

            return section;
        }

      
    }
}
