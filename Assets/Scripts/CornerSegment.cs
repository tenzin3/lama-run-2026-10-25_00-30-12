using UnityEngine;

public class CornerSegment : TrackSegment
{
    [Header("Corner Configuration")]
    [SerializeField] private TurnDirection _turnDirection = TurnDirection.Right;
    [SerializeField] private Vector3 _localPivot = new Vector3(0f, 0f, 18.4f);
    [SerializeField] private float _turnZoneStartLocalZ = 7f;
    [SerializeField] private float _turnZoneEndLocalZ = 21f;
    [SerializeField] private float _overrunEndLocalZ = 26.5f;

    // All corners currently in the world. Used during Meditation Flight, when Tenzin
    // flies above the corner trigger boxes and never touches them.
    public static readonly System.Collections.Generic.List<CornerSegment> Active =
        new System.Collections.Generic.List<CornerSegment>();

    private void OnEnable()
    {
        if (!Active.Contains(this)) Active.Add(this);
    }

    private void OnDisable()
    {
        Active.Remove(this);
    }

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

    // True from the start of the turn zone until the overrun edge, ignoring height,
    // so a flying player is still detected.
    public bool IsInFlightCornerZone(Vector3 worldPos)
    {
        if (HasTurned) return false;
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        return localPos.z >= _turnZoneStartLocalZ && localPos.z <= _overrunEndLocalZ && Mathf.Abs(localPos.x) <= 4f;
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
