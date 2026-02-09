using System;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 技能栏管理器 - 使用UGUI
/// </summary>
public class SkillBarUI : MonoBehaviour
{
    [Serializable]
    public class SkillSlotConfig
    {
        [Tooltip("技能id")]
        public int skillID;

        [Tooltip("技能数据")]
        public SkillGraphData skillData;

        [Tooltip("技能图标")]
        public Sprite icon;
    }

    [Header("引用")]
    public Player player;

    [Header("技能配置")]
    public List<SkillSlotConfig> skillConfigs = new List<SkillSlotConfig>();

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
        if (skillSlotPrefab == null || slotContainer == null) return;

        foreach (var config in skillConfigs)
        {
            // 实例化预制体
            GameObject slotObj = Instantiate(skillSlotPrefab, slotContainer);
            slotObj .gameObject.SetActive(true);
            var slotUI = slotObj.GetComponent<SkillSlotUI>();

            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<SkillSlotUI>();
            }

            // 初始化
            slotUI.Initialize(config, this);
            _skillSlots.Add(slotUI);

            // 授予技能并获取Spec
            if (player?.ownerASC != null && config.skillData != null)
            {
                player.ownerASC.GrantAbility(config.skillData);
                slotUI.AbilitySpec = player.ownerASC.Abilities.FindAbilityById(config.skillID);
            }
        }
    }


    /// <summary>
    /// 尝试激活技能
    /// </summary>
    public bool TryActivateSkill(SkillSlotUI slot)
    {
        if (player?.ownerASC == null || slot.AbilitySpec == null) return false;

      return  player.ownerASC.TryActivateAbility(slot.AbilitySpec,player.target.ownerASC);
    }
}

