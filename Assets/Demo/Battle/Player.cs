using System.Collections;
using System.Collections.Generic;
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

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        Vector2 moveDirection = new Vector2(horizontal, vertical).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            transform.position += (Vector3)moveDirection * ownerASC.Attributes.GetCurrentValue(AttrType.MoveSpeed) * Time.deltaTime;

            // 2D角色翻转朝向
            if (horizontal != 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = horizontal < 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }

  
}
