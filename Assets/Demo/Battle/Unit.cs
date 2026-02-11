using System.Collections;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public AbilitySystemComponent ownerASC;

    [Header("技能配置")]
    [Tooltip("从Luban技能表读取的技能ID列表")]
    public List<int> skillIds = new List<int>();

    protected virtual void Awake()
    {
        ownerASC = new AbilitySystemComponent(this.gameObject);

        // 生命相关属性
        if (!ownerASC.Attributes.HasAttribute(AttrType.Health))
            ownerASC.Attributes.AddAttribute(AttrType.Health, 1000000f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.MaxHealth))
            ownerASC.Attributes.AddAttribute(AttrType.MaxHealth, 1000000f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.HealthRegen))
            ownerASC.Attributes.AddAttribute(AttrType.HealthRegen, 1f);

        // 法力相关属性
        if (!ownerASC.Attributes.HasAttribute(AttrType.Mana))
            ownerASC.Attributes.AddAttribute(AttrType.Mana, 1000000f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.MaxMana))
            ownerASC.Attributes.AddAttribute(AttrType.MaxMana, 1000000f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.ManaRegen))
            ownerASC.Attributes.AddAttribute(AttrType.ManaRegen, 1f);

        // 战斗属性
        if (!ownerASC.Attributes.HasAttribute(AttrType.Attack))
            ownerASC.Attributes.AddAttribute(AttrType.Attack, 20f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.Defense))
            ownerASC.Attributes.AddAttribute(AttrType.Defense, 10f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.MagicPower))
            ownerASC.Attributes.AddAttribute(AttrType.MagicPower, 15f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.MagicDefense))
            ownerASC.Attributes.AddAttribute(AttrType.MagicDefense, 10f);

        // 速度相关属性
        if (!ownerASC.Attributes.HasAttribute(AttrType.MoveSpeed))
            ownerASC.Attributes.AddAttribute(AttrType.MoveSpeed, 5f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.AttackSpeed))
            ownerASC.Attributes.AddAttribute(AttrType.AttackSpeed, 1f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.CooldownReduction))
            ownerASC.Attributes.AddAttribute(AttrType.CooldownReduction, 0f);

        // 暴击相关属性
        if (!ownerASC.Attributes.HasAttribute(AttrType.CritRate))
            ownerASC.Attributes.AddAttribute(AttrType.CritRate, 0.05f);
        if (!ownerASC.Attributes.HasAttribute(AttrType.CritDamage))
            ownerASC.Attributes.AddAttribute(AttrType.CritDamage, 1.5f);

        // 根据技能ID列表从Luban表自动授予技能
        GrantAbilitiesFromTable();
    }

    /// <summary>
    /// 从Luban技能表读取数据并自动授予技能
    /// </summary>
    protected void GrantAbilitiesFromTable()
    {
        var tbSkill = LubanManager.Instance.Tables.TbSkill;
        foreach (var skillId in skillIds)
        {
            var skillData = tbSkill.GetOrDefault(skillId);
            if (skillData == null)
            {
                Debug.LogWarning($"[Unit] 技能表中找不到ID: {skillId}");
                continue;
            }

            var graphData = Resources.Load<SkillGraphData>(skillData.SkillGraphDataPath);
            if (graphData == null)
            {
                Debug.LogWarning($"[Unit] 无法加载SkillGraphData: {skillData.SkillGraphDataPath}");
                continue;
            }

            ownerASC.GrantAbility(graphData, skillId);
        }
    }
}
