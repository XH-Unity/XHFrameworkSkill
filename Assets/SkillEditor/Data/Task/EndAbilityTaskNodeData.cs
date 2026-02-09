using System;
using UnityEngine.Scripting.APIUpdating;

namespace SkillEditor.Data
{
    /// <summary>
    /// 结束技能任务节点数据
    /// 用于结束当前技能的执行
    /// </summary>
    [Serializable]
    public class EndAbilityTaskNodeData : TaskNodeData
    {
        /// <summary>
        /// 结束类型：正常结束或取消
        /// </summary>
        public EndAbilityType endType = EndAbilityType.Normal;
    }
}
