using SkillEditor.Data;
using SkillEditor.Runtime;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Unit _unit;
    private bool _isStunned;
    private GameplayTag _stunTag = new GameplayTag("Buff.DeBuff.Stun");

    void Awake()
    {
        _unit = GetComponent<Unit>();
    }

    void Start()
    {
        _unit.ownerASC.OwnedTags.OnTagAdded += OnTagAdded;
        _unit.ownerASC.OwnedTags.OnTagRemoved += OnTagRemoved;
    }

    void OnDestroy()
    {
        if (_unit?.ownerASC != null)
        {
            _unit.ownerASC.OwnedTags.OnTagAdded -= OnTagAdded;
            _unit.ownerASC.OwnedTags.OnTagRemoved -= OnTagRemoved;
        }
    }

    private void OnTagAdded(GameplayTag tag)
    {
        if (tag == _stunTag)
            _isStunned = true;
    }

    private void OnTagRemoved(GameplayTag tag)
    {
        if (tag == _stunTag)
            _isStunned = false;
    }

    void Update()
    {
        if (_isStunned) return;

        float horizontal = 0f;
        float vertical = 0f;

        if (Input.GetKey(KeyCode.W)) vertical += 1f;
        if (Input.GetKey(KeyCode.S)) vertical -= 1f;
        if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
        if (Input.GetKey(KeyCode.D)) horizontal += 1f;

        Vector2 moveDirection = new Vector2(horizontal, vertical).normalized;

        if (moveDirection.magnitude > 0.1f)
        {
            transform.position += (Vector3)moveDirection * _unit.ownerASC.Attributes.GetCurrentValue(AttrType.MoveSpeed) * Time.deltaTime;

            if (horizontal != 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = horizontal < 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }
}
