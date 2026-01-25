using System;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3IntExtension
{
    public static Vector3Int Cross(this Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    public static int Dot(this Vector3Int a, Vector3Int b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
        
    public static Vector3Int Rotate90(this Vector3Int v, Vector3Int unitAxis, int nRightTurns)
    {
        nRightTurns %= 4;
        var cos = Mathf.RoundToInt(Mathf.Cos(nRightTurns * Mathf.PI / 2));
        var sin = Mathf.RoundToInt(Mathf.Sin(nRightTurns * Mathf.PI / 2));
        
        if (unitAxis == Vector3Int.right)
        {
            var newY = v.y * cos - v.z * sin;
            var newZ = v.y * sin + v.z * cos;
            return new Vector3Int(v.x, newY, newZ);
        }
        
        if (unitAxis == Vector3Int.left)
        {
            var newY = -v.y * cos + v.z * sin;
            var newZ = -v.y * sin - v.z * cos;
            return new Vector3Int(v.x, newY, newZ);
        }

        if (unitAxis == Vector3Int.up)
        {
            var newX = v.x * cos + v.z * sin;
            var newZ = v.z * cos - v.x * sin;
            return new Vector3Int(newX, v.y, newZ);
        }
        
        if (unitAxis == Vector3Int.down)
        {
            var newX = -v.x * cos - v.z * sin;
            var newZ = -v.z * cos + v.x * sin;
            return new Vector3Int(newX, v.y, newZ);
        }

        if (unitAxis == Vector3Int.forward)
        {
            var newX = v.x * cos - v.y * sin;
            var newY = v.x * sin + v.y * cos;
            return new Vector3Int(newX, newY, v.z);
        }
        
        if (unitAxis == Vector3Int.back)
        {
            var newX = -v.x * cos + v.y * sin;
            var newY = -v.x * sin - v.y * cos;
            return new Vector3Int(newX, newY, v.z);
        }

        throw new ArgumentException($"{v} is not an unit axis vector!");
    }

    public static List<Vector3Int> Neighbours(this Vector3Int v)
    {
        var neighbours = new List<Vector3Int>
        {
            v + Vector3Int.up,
            v + Vector3Int.down,
            v + Vector3Int.left,
            v + Vector3Int.right,
            v + Vector3Int.forward,
            v + Vector3Int.back
        };
        return neighbours;
    }
}