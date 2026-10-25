using UnityEngine;

public enum ObstacleType
{
    LowBarrier,   // Jump over
    HighBeam,     // Slide under
    LaneBlock     // Switch lane
}

public class Obstacle : MonoBehaviour
{
    [SerializeField] private ObstacleType _obstacleType = ObstacleType.LaneBlock;

    public ObstacleType Type => _obstacleType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out var player))
        {
            player.HitObstacle(this);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
        {
            player.HitObstacle(this);
        }
    }
}
