using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 结束技能任务节点
    /// 这是一个终结节点，没有输出端口
    /// </summary>
    public class EndAbilityTaskNode : TaskNode<EndAbilityTaskNodeData>
    {
        private EnumField endTypeField;

        public EndAbilityTaskNode(Vector2 position) : base(NodeType.EndAbilityTask, position) { }

        protected override string GetNodeTitle() => "结束技能";
        protected override float GetNodeWidth() => 150;

        protected override void CreateTaskContent()
        {
            // 结束类型选择
            endTypeField = new EnumField("结束类型", EndAbilityType.Normal);
            ApplyFieldStyle(endTypeField);
            // 缩短标签宽度，让枚举值有更多空间
            var label = endTypeField.Q<Label>();
            if (label != null)
            {
                label.style.minWidth = 50;
                label.style.width = 50;
            }
            endTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.endType = (EndAbilityType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(endTypeField);
        }

        protected override void SyncTaskContentFromData()
        {
            if (TypedData == null) return;
            if (endTypeField != null)
                endTypeField.SetValueWithoutNotify(TypedData.endType);
        }
    }
}
