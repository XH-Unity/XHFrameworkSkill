using UnityEngine;

namespace SkillEditor.Data
{
    /// <summary>
    /// 属性类型枚举
    /// 策划在Excel属性表中配置属性ID和默认值，运行时读取
    /// Excel格式示例: [[1,100],[2,100],[3,20],[4,10]]
    /// </summary>
    public enum AttrType
    {
        [InspectorName("无")]
        None = 0,

        // ============ 生命相关 (1-99) ============
        [InspectorName("生命值")]
        Health = 1,
        [InspectorName("最大生命值")]
        MaxHealth = 2,
        [InspectorName("生命恢复")]
        HealthRegen = 3,

        // ============ 法力相关 (100-199) ============
        [InspectorName("法力值")]
        Mana = 100,
        [InspectorName("最大法力值")]
        MaxMana = 101,
        [InspectorName("法力恢复")]
        ManaRegen = 102,

        // ============ 战斗属性 (200-299) ============
        [InspectorName("攻击力")]
        Attack = 200,
        [InspectorName("防御力")]
        Defense = 201,
        [InspectorName("法术强度")]
        MagicPower = 202,
        [InspectorName("魔法抗性")]
        MagicDefense = 203,

        // ============ 速度相关 (300-399) ============
        [InspectorName("移动速度")]
        MoveSpeed = 300,
        [InspectorName("攻击速度")]
        AttackSpeed = 301,
        [InspectorName("冷却缩减")]
        CooldownReduction = 302,

        // ============ 暴击相关 (400-499) ============
        [InspectorName("暴击率")]
        CritRate = 400,
        [InspectorName("暴击伤害")]
        CritDamage = 401,

        // ============ 其他 (500+) ============
        [InspectorName("等级")]
        Level = 500,
        [InspectorName("经验值")]
        Experience = 501,

        // ============ Meta属性 (临时计算用) (900+) ============
        [InspectorName("受到伤害")]
        IncomingDamage = 900,
        [InspectorName("受到治疗")]
        IncomingHeal = 901,
    }
}
