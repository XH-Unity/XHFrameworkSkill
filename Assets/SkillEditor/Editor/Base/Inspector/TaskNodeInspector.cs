using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 任务节点Inspector基类 - 所有Task节点的Inspector继承此类
    /// 简化的Inspector，不显示Effect相关配置（标签、堆叠、持续时间等）
    /// </summary>
    public abstract class TaskNodeInspector : NodeInspectorBase
    {
        /// <summary>
        /// Task 节点默认显示节点目标
        /// </summary>
        protected override bool ShowTargetType => true;

        protected sealed override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var taskData = GetTaskNodeData(node);
            if (taskData == null) return;

            // 绘制子类的具体内容
            BuildTaskInspectorUI(container, node);
        }

        /// <summary>
        /// 子类实现具体的Task Inspector UI绘制
        /// </summary>
        protected abstract void BuildTaskInspectorUI(VisualElement container, SkillNodeBase node);

        /// <summary>
        /// 获取TaskNodeData
        /// </summary>
        protected virtual TaskNodeData GetTaskNodeData(SkillNodeBase node)
        {
            return node?.NodeData as TaskNodeData;
        }
    }
}
