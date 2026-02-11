using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public class Monster : Unit
{
    public Unit target;
    public AnimationComponent AnimationComponent;
    private GameplayAbilitySpec _normalAttackSpec;

    public override UnitType Type => UnitType.Monster;

    void Start()
    {
        ownerASC.OwnedTags.AddTag(new GameplayTag("unitType.monster"));

        var monsterData = LubanManager.Instance.Tables.TbMonster.GetOrDefault(id);
        if (monsterData != null && monsterData.ActiveSkill.Length > 0)
        {
            _normalAttackSpec = ownerASC.Abilities.FindAbilityById(monsterData.ActiveSkill[0]);
        }
    }

    void Update()
    {
        TryNormalAttack();

        if (target)
        {
            Vector3 scale = transform.localScale;
            scale.x = target.transform.position.x < transform.position.x ? 1 : -1;
            transform.localScale = scale;
        }
    }

    private void TryNormalAttack()
    {
        if (_normalAttackSpec == null || target == null) return;

        if (!_normalAttackSpec.IsRunning && !AnimationComponent._isStunned)
        {
            bool success = ownerASC.TryActivateAbility(_normalAttackSpec, target.ownerASC);

            if (!success)
            {
                AnimationComponent.PlayAnimation("Stand", true);
            }
        }
    }
}
