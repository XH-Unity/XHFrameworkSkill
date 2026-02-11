using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

/// <summary>
/// 单位属性GUI显示器
/// 在游戏运行时显示Player和Boss的属性面板
/// </summary>
public class UnitAttributeGUI : MonoBehaviour
{
    [Header("引用")]
    public Player player;
    public Monster monster;

    // GUI样式
    private GUIStyle boxStyle;
    private GUIStyle headerStyle;
    private GUIStyle labelStyle;
    private GUIStyle valueStyle;
    private bool stylesInitialized = false;

    // 滚动位置
    private Vector2 playerScrollPos;
    private Vector2 bossScrollPos;

    // 面板尺寸
    private const float PanelWidth = 200f;
    private const float PanelHeight = 500f;
    private const float Margin = 20f;

    void OnGUI()
    {
        InitStyles();

        // 绘制Player属性面板（左侧）
        if (player != null && player.ownerASC != null)
        {
            Rect playerRect = new Rect(Margin, Margin, PanelWidth, PanelHeight);
            DrawUnitPanel(playerRect, "Player 属性", player.ownerASC, ref playerScrollPos, new Color(0.2f, 0.6f, 0.2f, 0.9f));
        }

        // 绘制Boss属性面板（右侧）
        if (monster != null && monster.ownerASC != null)
        {
            Rect bossRect = new Rect(Screen.width - PanelWidth - Margin, Margin, PanelWidth, PanelHeight);
            DrawUnitPanel(bossRect, "Boss 属性", monster.ownerASC, ref bossScrollPos, new Color(0.6f, 0.2f, 0.2f, 0.9f));
        }
    }

    private void InitStyles()
    {
        if (stylesInitialized) return;

        boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTexture(2, 2, new Color(0.1f, 0.1f, 0.1f, 0.8f));

        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = Color.white;

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = new Color(0.9f, 0.9f, 0.9f);

        valueStyle = new GUIStyle(GUI.skin.label);
        valueStyle.fontSize = 16;
        valueStyle.alignment = TextAnchor.MiddleRight;
        valueStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        stylesInitialized = true;
    }

    private void DrawUnitPanel(Rect panelRect, string title, AbilitySystemComponent asc, ref Vector2 scrollPos, Color headerColor)
    {
        // 绘制背景
        GUI.Box(panelRect, "", boxStyle);

        // 绘制标题栏
        Rect headerRect = new Rect(panelRect.x, panelRect.y, panelRect.width, 30);
        GUI.DrawTexture(headerRect, MakeTexture(1, 1, headerColor));
        GUI.Label(headerRect, title, headerStyle);

        // 绘制Tags
        Rect tagsRect = new Rect(panelRect.x + 5, panelRect.y + 35, panelRect.width - 10, 26);
        string tags = GetTagsString(asc);
        GUI.Label(tagsRect, $"Tags: {tags}", labelStyle);

        // 属性列表区域
        Rect contentRect = new Rect(panelRect.x + 5, panelRect.y + 60, panelRect.width - 10, panelRect.height - 70);
        Rect viewRect = new Rect(0, 0, contentRect.width - 40, GetAttributeListHeight(asc));

        scrollPos = GUI.BeginScrollView(contentRect, scrollPos, viewRect);

        float y = 0;
        DrawAttributeCategory(ref y, viewRect.width, "生命相关", asc, AttrType.Health, AttrType.MaxHealth, AttrType.HealthRegen);
        DrawAttributeCategory(ref y, viewRect.width, "法力相关", asc, AttrType.Mana, AttrType.MaxMana, AttrType.ManaRegen);
        DrawAttributeCategory(ref y, viewRect.width, "战斗属性", asc, AttrType.Attack, AttrType.Defense, AttrType.MagicPower, AttrType.MagicDefense);
        DrawAttributeCategory(ref y, viewRect.width, "速度相关", asc, AttrType.MoveSpeed, AttrType.AttackSpeed, AttrType.CooldownReduction);
        DrawAttributeCategory(ref y, viewRect.width, "暴击相关", asc, AttrType.CritRate, AttrType.CritDamage);
        DrawAttributeCategory(ref y, viewRect.width, "其他", asc, AttrType.Level, AttrType.Experience);

        GUI.EndScrollView();
    }

    private void DrawAttributeCategory(ref float y, float width, string categoryName, AbilitySystemComponent asc, params AttrType[] attrTypes)
    {
        // 分类标题
        GUI.Label(new Rect(0, y, width, 28), $"── {categoryName} ──", headerStyle);
        y += 30;

        foreach (var attrType in attrTypes)
        {
            var attr = asc.Attributes.GetAttribute(attrType);
            if (attr != null)
            {
                DrawAttributeRow(ref y, width, attrType, attr);
            }
        }

        y += 5; // 分类间距
    }

    private void DrawAttributeRow(ref float y, float width, AttrType attrType, Attribute attr)
    {
        string attrName = GetAttributeName(attrType);
        string valueText;

        // 特殊格式化
        if (attrType == AttrType.CritRate || attrType == AttrType.CooldownReduction)
        {
            valueText = $"{attr.CurrentValue * 100:F1}%";
        }
        else if (attrType == AttrType.CritDamage)
        {
            valueText = $"{attr.CurrentValue * 100:F0}%";
        }
        else if (attrType == AttrType.Health || attrType == AttrType.Mana)
        {
            var maxAttr = attrType == AttrType.Health ? AttrType.MaxHealth : AttrType.MaxMana;
            float maxValue = attr.CurrentValue; // 默认值
            var maxAttribute = GetAttributeFromASC(attrType == AttrType.Health ? AttrType.MaxHealth : AttrType.MaxMana, attr);
            valueText = $"{attr.CurrentValue:F0}";
        }
        else
        {
            valueText = $"{attr.CurrentValue:F1}";
        }

        // 显示基础值和当前值的差异
        if (Mathf.Abs(attr.CurrentValue - attr.BaseValue) > 0.01f)
        {
            float diff = attr.CurrentValue - attr.BaseValue;
            string diffStr = diff > 0 ? $"+{diff:F1}" : $"{diff:F1}";
            valueText += $" ({diffStr})";
        }

        GUI.Label(new Rect(5, y, width * 0.5f, 26), attrName, labelStyle);
        GUI.Label(new Rect(width * 0.5f, y, width * 0.5f - 10, 26), valueText, valueStyle);

        y += 25;
    }

    private Attribute GetAttributeFromASC(AttrType attrType, Attribute currentAttr)
    {
        // 这个方法用于获取关联属性，但由于我们没有直接访问ASC的引用
        // 在实际使用中，应该通过ASC来获取
        return null;
    }

    private string GetAttributeName(AttrType attrType)
    {
        switch (attrType)
        {
            case AttrType.Health: return "生命值";
            case AttrType.MaxHealth: return "最大生命";
            case AttrType.HealthRegen: return "生命恢复";
            case AttrType.Mana: return "法力值";
            case AttrType.MaxMana: return "最大法力";
            case AttrType.ManaRegen: return "法力恢复";
            case AttrType.Attack: return "攻击力";
            case AttrType.Defense: return "防御力";
            case AttrType.MagicPower: return "法术强度";
            case AttrType.MagicDefense: return "魔法抗性";
            case AttrType.MoveSpeed: return "移动速度";
            case AttrType.AttackSpeed: return "攻击速度";
            case AttrType.CooldownReduction: return "冷却缩减";
            case AttrType.CritRate: return "暴击率";
            case AttrType.CritDamage: return "暴击伤害";
            case AttrType.Level: return "等级";
            case AttrType.Experience: return "经验值";
            default: return attrType.ToString();
        }
    }

    private string GetTagsString(AbilitySystemComponent asc)
    {
        if (asc.OwnedTags == null || asc.OwnedTags.IsEmpty) return "无";

        var tags = new List<string>();
        foreach (var tag in asc.OwnedTags.Tags)
        {
            tags.Add(tag.Name);
        }

        return tags.Count > 0 ? string.Join(", ", tags) : "无";
    }

    private float GetAttributeListHeight(AbilitySystemComponent asc)
    {
        // 6个分类，每个分类标题22px + 间距5px
        // 估算每个属性20px
        return 6 * 27 + 17 * 20 + 50; // 分类 + 属性 + 额外空间
    }

    private Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
