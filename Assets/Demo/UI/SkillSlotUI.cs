using System;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class SkillSlotUI : MonoBehaviour
{
    [Header("UI组件引用")]
    public Image iconImage;
    public Image cooldownMask;
    public Text cooldownText;
    public Text nameText;
    public Button button;

    // 运行时数据
    public int SkillId { get; private set; }
    public GameplayAbilitySpec AbilitySpec { get; set; }

    private SkillBarUI _manager;

    /// <summary>
    /// 初始化技能槽
    /// </summary>
    public void Initialize(int skillId, string skillName, Sprite icon, SkillBarUI manager)
    {
        SkillId = skillId;
        _manager = manager;

        // 设置图标
        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
        }

        // 设置名称
        if (nameText != null)
        {
            nameText.text = skillName;
        }

        // 绑定按钮点击
        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        // 初始化冷却显示
        SetCooldown(false, 0f, 0f);
    }

    public void Update()
    {
        if (AbilitySpec!=null)
        {
            // 从ASC查询冷却效果
            GameplayEffectSpec cooldownEffect = AbilitySpec.GetRemainingCooldown();

            if (cooldownEffect != null)
            {
                float remaining = cooldownEffect.RemainingTime;
                float duration = cooldownEffect.Duration;
                float progress = duration > 0 ? remaining / duration : 0f;

                SetCooldown(true, progress, remaining);
            }
            else
            {
                SetCooldown(false, 0f, 0f);
            }
        }
    }

    /// <summary>
    /// 设置冷却显示
    /// </summary>
    public void SetCooldown(bool isOnCooldown, float progress, float remainingTime)
    {
        if (cooldownMask != null)
        {
            cooldownMask.gameObject.SetActive(isOnCooldown);
            // 使用 fillAmount 实现转圈效果（需要Image设置为Filled类型）
            cooldownMask.fillAmount = progress;
        }

        if (cooldownText != null)
        {
            cooldownText.gameObject.SetActive(isOnCooldown && remainingTime > 0);
            if (isOnCooldown)
            {
                cooldownText.text = remainingTime > 1f ? $"{remainingTime:F0}" : $"{remainingTime:F1}";
            }
        }
    }

    private void OnClick()
    {
       _manager?.TryActivateSkill(this);
    }
}
