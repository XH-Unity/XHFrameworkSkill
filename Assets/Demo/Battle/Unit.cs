using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public enum UnitType
{
    Hero,
    Monster,
}

public class Unit : MonoBehaviour
{
    public AbilitySystemComponent ownerASC;

    [Header("单位配置")]
    public int id;

    public virtual UnitType Type => UnitType.Hero;

    protected virtual void Awake()
    {
        ownerASC = new AbilitySystemComponent(this.gameObject);

        UnitManager.Instance.Register(this);

        InitFromTable();
    }

    protected virtual void OnDestroy()
    {
        UnitManager.Instance.Unregister(this);
    }

    private void InitFromTable()
    {
        var tables = LubanManager.Instance.Tables;
        (int, int)[] attributes = null;
        int[] activeSkills = null;
        int[] passiveSkills = null;

        if (Type == UnitType.Hero)
        {
            var data = tables.TbHero.GetOrDefault(id);
            if (data == null)
            {
                Debug.LogWarning($"[Unit] TbHero中找不到ID: {id}");
                return;
            }
            attributes = data.InitialAttribute;
            activeSkills = data.ActiveSkill;
            passiveSkills = data.PassiveSkill;
        }
        else
        {
            var data = tables.TbMonster.GetOrDefault(id);
            if (data == null)
            {
                Debug.LogWarning($"[Unit] TbMonster中找不到ID: {id}");
                return;
            }
            attributes = data.InitialAttribute;
            activeSkills = data.ActiveSkill;
            passiveSkills = data.PassiveSkill;
        }

        InitAttributes(attributes);
        GrantSkills(activeSkills);
        GrantSkills(passiveSkills);
    }

    private void InitAttributes((int, int)[] attributes)
    {
        if (attributes == null) return;

        foreach (var (typeId, value) in attributes)
        {
            var attrType = (AttrType)typeId;
            if (!ownerASC.Attributes.HasAttribute(attrType))
                ownerASC.Attributes.AddAttribute(attrType, value);
        }
    }

    private void GrantSkills(int[] skillIds)
    {
        if (skillIds == null) return;

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
