using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    [SerializeField] private float _length = 30f;
    [SerializeField] private Vector3 _localEndPosition = new Vector3(0f, 0f, 30f);
    [SerializeField] private Vector3 _localEndRotation = Vector3.zero;

    public float Length => _length;
    public virtual bool IsCorner => false;

    public float StartZ => transform.position.z;
    public float EndZ => transform.position.z + _length;

    public float PathEndDistance { get; set; }

    public virtual Vector3 EndPosition
    {
        get
        {
            Vector3 offset = _localEndPosition == Vector3.zero ? new Vector3(0f, 0f, _length) : _localEndPosition;
            return transform.TransformPoint(offset);
        }
    }

    public virtual Quaternion EndRotation => transform.rotation * Quaternion.Euler(_localEndRotation);

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 center = transform.position + new Vector3(0f, 0.1f, _length * 0.5f);
        Gizmos.DrawWireCube(center, new Vector3(6f, 0.2f, _length));
    }
}
