using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 任务节点数据基类 - 用于执行特定任务的节点
    /// 特点：瞬时执行、无属性修改、无堆叠、无持续时间
    /// 与 EffectNodeData 的区别：不需要标签系统、持续时间、堆叠等复杂配置
    /// </summary>
    [Serializable]
    public abstract class TaskNodeData : NodeData
    {
        // 只继承 NodeData 的基础字段: guid, nodeType, position, targetType
        // 不需要 EffectNodeData 的任何字段
    }
}
