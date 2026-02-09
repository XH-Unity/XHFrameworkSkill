using System;
using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 飘字Cue节点数据
    /// </summary>
    [Serializable]
    public class FloatingTextCueNodeData : CueNodeData
    {
        /// <summary>
        /// 位置来源
        /// </summary>
        public PositionSourceType positionSource = PositionSourceType.ParentInput;

        /// <summary>
        /// 挂点名称（为空则使用角色位置+偏移）
        /// </summary>
        public string positionBindingName = "";

        /// <summary>
        /// 飘字类型
        /// </summary>
        public FloatingTextType textType = FloatingTextType.Damage;

        /// <summary>
        /// 固定文本（当不使用动态数据时显示）
        /// </summary>
        public string fixedText = "";

        /// <summary>
        /// 从上下文获取数据的Key（如 "DamageResult", "HealResult" 等）
        /// 为空时使用固定文本
        /// </summary>
        public string contextDataKey = "DamageResult";

        /// <summary>
        /// 文本颜色（普通伤害）
        /// </summary>
        public Color textColor = Color.white;

        /// <summary>
        /// 暴击颜色
        /// </summary>
        public Color criticalColor = new Color(1f, 0.5f, 0f);

        /// <summary>
        /// Miss颜色
        /// </summary>
        public Color missColor = Color.gray;

        /// <summary>
        /// 字体大小
        /// </summary>
        public float fontSize = 45f;

        /// <summary>
        /// 暴击字体大小（通常比普通大）
        /// </summary>
        public float criticalFontSize = 60f;

        /// <summary>
        /// 飘字持续时间
        /// </summary>
        public float duration = 1.5f;

        /// <summary>
        /// 飘字偏移（相对于目标位置）
        /// </summary>
        public Vector2 offset = new Vector2(0, 0f);

        /// <summary>
        /// 飘字移动方向和距离
        /// </summary>
        public Vector2 moveDirection = new Vector2(0, 1f);
    }

    /// <summary>
    /// 飘字类型
    /// </summary>
    public enum FloatingTextType
    {
        [InspectorName("伤害")]
        Damage,
        [InspectorName("治疗")]
        Heal,
        [InspectorName("状态")]
        Status,
        [InspectorName("经验")]
        Experience,
        [InspectorName("金币")]
        Gold,
        [InspectorName("自定义")]
        Custom
    }

    /// <summary>
    /// 伤害结果结构体 - 用于传递给飘字Cue
    /// </summary>
    [Serializable]
    public struct DamageResult
    {
        /// <summary>
        /// 伤害值
        /// </summary>
        public float Damage;

        /// <summary>
        /// 是否暴击
        /// </summary>
        public bool IsCritical;

        /// <summary>
        /// 是否闪避/Miss
        /// </summary>
        public bool IsMiss;

        /// <summary>
        /// 伤害类型
        /// </summary>
        public DamageType DamageType;

        public DamageResult(float damage, bool isCritical = false, bool isMiss = false, DamageType damageType = DamageType.Physical)
        {
            Damage = damage;
            IsCritical = isCritical;
            IsMiss = isMiss;
            DamageType = damageType;
        }
    }
    
}
