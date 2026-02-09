using UnityEngine.UIElements;

using SkillEditor.Data;
namespace SkillEditor.Editor
{
    public class DefaultNodeInspector : NodeInspectorBase
    {
        protected override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var label = new Label("选择节点以查看属性")
            {
                style = { fontSize = 12, marginTop = 10 }
            };
            container.Add(label);
        }
    }
}