using UnityEngine.UIElements;
using UnityEditor.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 结束技能任务节点Inspector
    /// </summary>
    public class EndAbilityTaskNodeInspector : TaskNodeInspector
    {
        // 结束技能不需要显示节点目标
        protected override bool ShowTargetType => false;

        protected override void BuildTaskInspectorUI(VisualElement container, SkillNodeBase node)
        {
            if (node is EndAbilityTaskNode endAbilityNode)
            {
                var data = endAbilityNode.TypedData;
                if (data == null) return;

                // 结束类型
                var endTypeField = new EnumField("结束类型", data.endType);
                ApplyEnumFieldStyle(endTypeField);
                endTypeField.RegisterValueChangedCallback(evt =>
                {
                    data.endType = (EndAbilityType)evt.newValue;
                    endAbilityNode.SyncUIFromData();
                });
                container.Add(endTypeField);
            }
        }
    }
}
