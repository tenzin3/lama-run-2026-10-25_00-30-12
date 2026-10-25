using UnityEngine;

[RequireComponent(typeof(Collider))]
public class OffEdgeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.FallOffEdge();
        }
    }
}
