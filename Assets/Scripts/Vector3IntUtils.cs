using UnityEngine;

public static class Vector3IntUtils
{
    public static Vector3Int[] Directions
    {
        get
        {
            return new[]
            {
                Vector3Int.up,
                Vector3Int.down,
                Vector3Int.right,
                Vector3Int.left, 
                Vector3Int.forward,
                Vector3Int.back
            };   
        }
    }
}