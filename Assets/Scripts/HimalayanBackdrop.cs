using UnityEngine;

public class HimalayanBackdrop : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private float _yOffset = 0f;

    private void Start()
    {
        if (_target == null)
        {
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null)
            {
                _target = pc.transform;
            }
        }
    }

    private void LateUpdate()
    {
        if (_target != null)
        {
            transform.position = new Vector3(_target.position.x, _yOffset, _target.position.z);
        }
    }
}
