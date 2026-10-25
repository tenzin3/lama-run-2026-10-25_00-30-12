using UnityEngine;

public class CornerSegment : TrackSegment
{
    [Header("Corner Configuration")]
    [SerializeField] private TurnDirection _turnDirection = TurnDirection.Right;
    [SerializeField] private Vector3 _localPivot = new Vector3(0f, 0f, 18.4f);
    [SerializeField] private float _turnZoneStartLocalZ = 7f;
    [SerializeField] private float _turnZoneEndLocalZ = 21f;
    [SerializeField] private float _overrunEndLocalZ = 26.5f;

    public override bool IsCorner => true;
    public TurnDirection Direction => _turnDirection;
    public bool HasTurned { get; private set; }

    public Vector3 PivotPosition => transform.TransformPoint(_localPivot);

    public Quaternion TargetHeadingRotation =>
        transform.rotation * Quaternion.Euler(0f, _turnDirection == TurnDirection.Right ? 90f : -90f, 0f);

    public Vector3 NewForwardDirection => TargetHeadingRotation * Vector3.forward;
    public Vector3 NewRightDirection => TargetHeadingRotation * Vector3.right;

    public bool IsPlayerInTurnZone(Vector3 worldPos)
    {
        if (HasTurned) return false;
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return localPos.z >= _turnZoneStartLocalZ && localPos.z <= _turnZoneEndLocalZ && Mathf.Abs(localPos.x) <= 4f;
    }

    public bool HasPlayerRunOffEdge(Vector3 worldPos)
    {
        if (HasTurned) return false;
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return localPos.z >= _overrunEndLocalZ;
    }

    public void OnTurnSuccess()
    {
        HasTurned = true;
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;
        Vector3 pivotWorld = PivotPosition;
        Gizmos.DrawWireSphere(pivotWorld, 1f);

        Gizmos.color = Color.cyan;
        Vector3 zoneCenter = transform.TransformPoint(new Vector3(0f, 0.5f, (_turnZoneStartLocalZ + _turnZoneEndLocalZ) * 0.5f));
        float zoneLength = _turnZoneEndLocalZ - _turnZoneStartLocalZ;
        Gizmos.DrawWireCube(zoneCenter, new Vector3(6.8f, 1f, zoneLength));
    }
}
