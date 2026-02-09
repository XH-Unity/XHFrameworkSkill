using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 放置物效果节点数据
    /// </summary>
    [Serializable]
    public class PlacementEffectNodeData : EffectNodeData
    {
        public PlacementEffectNodeData()
        {
            // 放置物默认是持续效果
            durationType = EffectDurationType.Duration;
        }

        // ============ 放置物设置 ============

        /// <summary>
        /// 放置物位置来源
        /// </summary>
        public PositionSourceType positionSource = PositionSourceType.MainTarget;

        /// <summary>
        /// 放置物挂点位置（如果有挂点，放置物会生成在目标的该挂点位置）
        /// </summary>
        public string positionBindingName;

        /// <summary>
        /// 放置物预制体
        /// </summary>
        public GameObject placementPrefab;

        // ============ 碰撞设置 ============

        /// <summary>
        /// 是否启用碰撞检测
        /// </summary>
        public bool enableCollision = false;

        /// <summary>
        /// 碰撞检测范围（半径）
        /// </summary>
        public float collisionRadius = 2f;

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
