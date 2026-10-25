using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CornerTriggerZone : MonoBehaviour
{
    [SerializeField] private CornerSegment _cornerSegment;

    private void Awake()
    {
        if (_cornerSegment == null)
        {
            _cornerSegment = GetComponentInParent<CornerSegment>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.EnterCorner(_cornerSegment);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.ExitCorner(_cornerSegment);
        }
    }
}
