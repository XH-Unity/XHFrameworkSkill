using System.Collections.Generic;
using SkillEditor.Runtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 技能栏管理器 - 使用UGUI
/// </summary>
public class SkillBarUI : MonoBehaviour
{
    [Header("引用")]
    public Player player;

    [Header("UI引用")]
    [Tooltip("技能槽预制体")]
    public GameObject skillSlotPrefab;

    [Tooltip("技能槽父物体")]
    public Transform slotContainer;

    // 运行时技能槽数据
    private List<SkillSlotUI> _skillSlots = new List<SkillSlotUI>();

    void Start()
    {
        skillSlotPrefab.gameObject.SetActive(false);
        CreateSkillSlots();
    }

    /// <summary>
    /// 创建技能槽UI
    /// </summary>
    private void CreateSkillSlots()
    {
        if (skillSlotPrefab == null || slotContainer == null || player == null) return;

        var tbSkill = LubanManager.Instance.Tables.TbSkill;

        foreach (var skillId in player.skillIds)
        {
            var skillData = tbSkill.GetOrDefault(skillId);
            if (skillData == null) continue;

            // 从Resources加载图标
            Sprite icon = Resources.Load<Sprite>(skillData.IconPath);

            // 实例化预制体
            GameObject slotObj = Instantiate(skillSlotPrefab, slotContainer);
            slotObj.gameObject.SetActive(true);
            var slotUI = slotObj.GetComponent<SkillSlotUI>();

            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<SkillSlotUI>();
            }

            // 初始化
            slotUI.Initialize(skillId, skillData.Name, icon, this);
            _skillSlots.Add(slotUI);

            // 从player的ASC中查找已授予的技能Spec
            if (player.ownerASC != null)
            {
                slotUI.AbilitySpec = player.ownerASC.Abilities.FindAbilityById(skillId);
            }
        }
    }

    /// <summary>
    /// 尝试激活技能
    /// </summary>
    public bool TryActivateSkill(SkillSlotUI slot)
    {
        if (player?.ownerASC == null || slot.AbilitySpec == null) return false;

        return player.ownerASC.TryActivateAbility(slot.AbilitySpec, player.target.ownerASC);
    }
}
