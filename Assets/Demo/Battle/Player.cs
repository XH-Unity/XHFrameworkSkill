using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public class Player : Unit
{
    public Unit target;

    void Start()
    {
        ownerASC.OwnedTags.AddTag(new GameplayTag("unitType.hero"));
    }
}
