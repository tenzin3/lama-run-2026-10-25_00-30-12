using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    [SerializeField] private float _length = 30f;

    public float Length => _length;

    public float StartZ => transform.position.z;
    public float EndZ => transform.position.z + _length;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + new Vector3(0f, 0.1f, _length * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(6f, 0.2f, _length));
    }
}
