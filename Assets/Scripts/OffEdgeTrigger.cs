using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OffEdgeTrigger : MonoBehaviour
{
    [Tooltip("If true, only triggers fall if player is grounded (used for gaps where jumping clears it).")]
    [SerializeField] private bool _onlyWhenGrounded = false;

    public bool OnlyWhenGrounded
    {
        get => _onlyWhenGrounded;
        set => _onlyWhenGrounded = value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            if (_onlyWhenGrounded && !player.IsGrounded) return;
            player.FallOffEdge();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (_onlyWhenGrounded && other.TryGetComponent<PlayerController>(out var player))
        {
            if (player.IsGrounded)
            {
                player.FallOffEdge();
            }
        }
    }
}
