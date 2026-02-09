namespace SkillEditor.Runtime
{
    /// <summary>
    /// 技能状态枚举 - 对应GAS的技能生命周期状态
    /// </summary>
    public enum AbilityState
    {
        /// <summary>
        /// 未激活 - 技能已授予但未执行
        /// </summary>
        Inactive,

        /// <summary>
        /// 激活中 - 技能正在执行
        /// </summary>
        Active,

        /// <summary>
        /// 已结束 - 技能正常结束
        /// </summary>
        Ended,

        /// <summary>
        /// 已取消 - 技能被取消
        /// </summary>
        Cancelled
    }
}
