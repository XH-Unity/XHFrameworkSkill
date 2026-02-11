using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    private Transform _target;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var player = UnitManager.Instance.GetUnit<Player>();
        if (player != null)
        {
            SetFollowTarget(player.transform);
        }
    }

    void LateUpdate()
    {
        if (_target == null) return;

        Vector3 pos = _target.position;
        pos.z = transform.position.z;
        transform.position = pos;
    }

    public void SetFollowTarget(Transform target)
    {
        _target = target;
    }
}
