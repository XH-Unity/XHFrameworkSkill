using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 飘字Cue节点
    /// </summary>
    public class FloatingTextCueNode : CueNode<FloatingTextCueNodeData>
    {
        private EnumField textTypeField;
        private TextField contextKeyField;
        private TextField fixedTextField;

        public FloatingTextCueNode(Vector2 position) : base(NodeType.FloatingTextCue, position)
        {
        }

        protected override string GetNodeTitle() => "飘字";
        protected override float GetNodeWidth() => 120;

        protected override void CreateContent()
        {
            // 飘字类型
            textTypeField = new EnumField("类型", FloatingTextType.Damage);
            ApplyFieldStyle(textTypeField);
            textTypeField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.textType = (FloatingTextType)evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(textTypeField);

            // 上下文数据Key
            contextKeyField = new TextField("数据Key") { value = "Damage" };
            ApplyFieldStyle(contextKeyField);
            contextKeyField.RegisterValueChangedCallback(evt =>
            {
                if (TypedData != null)
                {
                    TypedData.contextDataKey = evt.newValue;
                    NotifyDataChanged();
                }
            });
            mainContainer.Add(contextKeyField);
        }

        public override void LoadData(NodeData data)
        {
            base.LoadData(data);
            if (textTypeField != null) textTypeField.SetValueWithoutNotify(TypedData?.textType ?? FloatingTextType.Damage);
            if (contextKeyField != null) contextKeyField.SetValueWithoutNotify(TypedData?.contextDataKey ?? "Damage");
            if (fixedTextField != null) fixedTextField.SetValueWithoutNotify(TypedData?.fixedText ?? "");
        }

        public override void SyncUIFromData()
        {
            if (TypedData == null) return;
            if (textTypeField != null) textTypeField.SetValueWithoutNotify(TypedData.textType);
            if (contextKeyField != null) contextKeyField.SetValueWithoutNotify(TypedData.contextDataKey ?? "");
            if (fixedTextField != null) fixedTextField.SetValueWithoutNotify(TypedData.fixedText ?? "");
        }
    }
}
