using System.Collections;
using System.Collections.Generic;
using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public class Boss : Unit
{
    public string skillName;
    public SkillGraphData SkillGraphData;
    public Unit target;
    public AnimationComponent AnimationComponent;
    private GameplayAbilitySpec _normalAttackSpec;

    void Start()
    {
        ownerASC.OwnedTags.AddTag(new GameplayTag("unitType.boss"));

        // 授予普攻技能
        if (SkillGraphData != null)
        {
            _normalAttackSpec = ownerASC.GrantAbility(SkillGraphData);
        }
    }

    void Update()
    {
        // 持续尝试对target释放普攻
        TryNormalAttack();


        if (target)
        {
            Vector3 scale = transform.localScale;
            scale.x = target.transform.position.x < transform.position.x ? 1 : -1;
            transform.localScale = scale;

        }
    }

    /// <summary>
    /// 尝试释放普攻
    /// </summary>
    private void TryNormalAttack()
    {
        if (_normalAttackSpec == null || target == null) return;

        // 如果技能不在运行中 且 不是眩晕状态
        if (!_normalAttackSpec.IsRunning && !AnimationComponent._isStunned)
        {
            // 尝试激活技能
            bool success = ownerASC.TryActivateAbility(_normalAttackSpec, target.ownerASC);

            // 释放失败（可能是CD中），播放Stand动画
            if (!success)
            {
                AnimationComponent.PlayAnimation("Stand", true);
            }
        }
    }
}
