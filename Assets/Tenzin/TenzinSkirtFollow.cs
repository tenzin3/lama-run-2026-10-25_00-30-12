using UnityEngine;

/// <summary>Drives the four skirt bones after Humanoid retargeting.</summary>
[DefaultExecutionOrder(100)]
public sealed class TenzinSkirtFollow : MonoBehaviour
{
    public Transform[] legs = new Transform[4];
    public Transform[] skirt = new Transform[4];
    void LateUpdate() { Apply(); }
    public void Apply()
    {
        for (int i = 0; i < Mathf.Min(legs.Length, skirt.Length); i++)
            if (legs[i] && skirt[i])
                skirt[i].SetPositionAndRotation(legs[i].position, legs[i].rotation);
    }
}
