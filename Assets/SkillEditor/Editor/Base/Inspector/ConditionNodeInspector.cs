using UnityEngine;
using UnityEngine.UIElements;

using SkillEditor.Data;

namespace SkillEditor.Editor
{
    /// <summary>
    /// 条件节点Inspector基类 - 所有Condition节点的Inspector继承此类
    /// 简化的Inspector，不显示Effect相关配置（标签、堆叠、持续时间等）
    /// </summary>
    public abstract class ConditionNodeInspector : NodeInspectorBase
    {
        /// <summary>
        /// Condition 节点默认显示节点目标
        /// </summary>
        protected override bool ShowTargetType => true;

        protected sealed override void BuildInspectorUI(VisualElement container, SkillNodeBase node)
        {
            var conditionData = GetConditionNodeData(node);
            if (conditionData == null) return;

            // 绘制子类的具体内容
            BuildConditionInspectorUI(container, node);
        }

        /// <summary>
        /// 子类实现具体的Condition Inspector UI绘制
        /// </summary>
        protected abstract void BuildConditionInspectorUI(VisualElement container, SkillNodeBase node);

        /// <summary>
        /// 获取ConditionNodeData
        /// </summary>
        protected virtual ConditionNodeData GetConditionNodeData(SkillNodeBase node)
        {
            return node?.NodeData as ConditionNodeData;
        }
    }
}
