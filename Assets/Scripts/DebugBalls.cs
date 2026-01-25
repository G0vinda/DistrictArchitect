using System;
using UnityEngine;

public class DebugBalls : MonoBehaviour
{
    public int Count { get; set; }

    private readonly Vector3[] ballOffsets =
        { new(0, 0, 0), new(-0.3f, 0, 0), new(0.3f, 0, 0), new(0f, 0, 0.3f), new(0f, 0, -0.3f) };

    private void OnDrawGizmos()
    {
        for (int i = 0; i < Count; i++)
        {
            Gizmos.DrawSphere(transform.position + ballOffsets[i], 0.1f);
        }
    }
}