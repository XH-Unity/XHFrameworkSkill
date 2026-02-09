using System;

namespace SkillEditor.Data
{
    /// <summary>
    /// 位移效果节点数据
    /// 用于吸引/击退目标的持续位移效果
    /// </summary>
    [Serializable]
    public class DisplaceEffectNodeData : EffectNodeData
    {
        /// <summary>
        /// 位移类型：吸引/击退/吸引到指定点
        /// </summary>
        public DisplaceType displaceType = DisplaceType.Push;

        /// <summary>
        /// 移动速度（单位/秒）
        /// </summary>
        public float speed = 10f;

        /// <summary>
        /// 最大位移距离
        /// </summary>
        public float distance = 5f;

        /// <summary>
        /// 吸引时的最小距离（到达后停止）
        /// </summary>
        public float minDistance = 0.5f;

        /// <summary>
        /// PullToPoint 模式的目标点位置来源
        /// </summary>
        public PositionSourceType pointSource = PositionSourceType.Caster;

        /// <summary>
        /// PullToPoint 模式的目标点挂点名称
        /// </summary>
        public string pointBindingName = "";
        
        
        public DisplaceEffectNodeData()
        {
            // 普通CD是持续类型
            durationType = EffectDurationType.Duration;
 
            // CD不随技能结束而取消，由ASC管理生命周期
            cancelOnAbilityEnd = false;
        }
    }
}
