using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 投射物目标类型
    /// </summary>
    public enum ProjectileTargetType
    {
        [InspectorName("点")]
        Position,
        [InspectorName("单位")]
        Unit
    }

    /// <summary>
    /// 投射物效果节点数据
    /// </summary>
    [Serializable]
    public class ProjectileEffectNodeData : EffectNodeData
    {
        public ProjectileEffectNodeData()
        {
            // 投射物效果必须是永久类型，生命周期由投射物控制
            durationType = EffectDurationType.Infinite;
        }

        // ============ 发射设置 ============

        /// <summary>
        /// 发射位置来源
        /// </summary>
        public PositionSourceType launchPositionSource = PositionSourceType.Caster;

        /// <summary>
        /// 发射位置挂点名称，空则使用目标坐标位置
        /// </summary>
        public string launchBindingName;

        // ============ 目标设置 ============

        /// <summary>
        /// 目标位置来源
        /// </summary>
        public PositionSourceType targetPositionSource = PositionSourceType.MainTarget;

        /// <summary>
        /// 目标受击挂点名称，空则使用目标坐标位置
        /// </summary>
        public string targetBindingName;

        /// <summary>
        /// 投射物目标类型（点/单位）
        /// </summary>
        public ProjectileTargetType projectileTargetType = ProjectileTargetType.Unit;

        /// <summary>
        /// 是否飞跃（飞过目标点继续飞）- 仅点模式有效
        /// </summary>
        public bool flyOver = false;

        /// <summary>
        /// 上下偏移角度 - 仅点模式有效
        /// </summary>
        public float offsetAngle = 0f;

        /// <summary>
        /// 曲线参数（0=直线，>0=曲线弧度）
        /// 单位模式：始终可用
        /// 点模式：非飞跃时可用
        /// </summary>
        public float curveHeight = 0f;

        // ============ 投射物属性 ============

        /// <summary>
        /// 飞行物预制体
        /// </summary>
        public GameObject projectilePrefab;

        /// <summary>
        /// 飞行速度
        /// </summary>
        public float speed = 10f;

        /// <summary>
        /// 最大飞行距离（-1为无限，仅点模式有效）
        /// </summary>
        public float maxDistance = -1f;

        /// <summary>
        /// 碰撞半径
        /// </summary>
        public float collisionRadius = 0.5f;

        /// <summary>
        /// 是否穿透
        /// </summary>
        public bool isPiercing = false;

        /// <summary>
        /// 最大穿透数量（穿透模式下有效）
        /// </summary>
        public int maxPierceCount = 1;

        /// <summary>
        /// 碰撞目标标签
        /// </summary>
        public GameplayTagSet collisionTargetTags;

        /// <summary>
        /// 碰撞排除标签
        /// </summary>
        public GameplayTagSet collisionExcludeTags;
    }
}
