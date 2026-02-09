using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 条件节点数据基类 - 用于条件判断分支的节点
    /// 特点：瞬时执行、返回布尔结果、有"是/否"两个输出端口
    /// 与 EffectNodeData 的区别：不需要标签系统、持续时间、堆叠等复杂配置
    /// </summary>
    [Serializable]
    public abstract class ConditionNodeData : NodeData
    {
        // 只继承 NodeData 的基础字段: guid, nodeType, position, targetType
        // 条件节点通过"是/否"两个输出端口实现分支逻辑
    }
}
