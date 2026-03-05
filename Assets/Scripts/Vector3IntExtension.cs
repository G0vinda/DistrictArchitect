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
    
    public static int Get90RotationsAroundYTo(this Vector3Int from, Vector3Int to)
    {
        var rotatedFrom = from;
        for (int rotationsNeeded = 0; rotationsNeeded < 4; rotationsNeeded++)
        {
            if (rotatedFrom == to)
                return rotationsNeeded;

            rotatedFrom = rotatedFrom.Rotate90(Vector3Int.up, 1);            
        }
        
        throw new ArgumentException($"Vector {to} is not a 90 degree rotation of vector {from}.");
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

    public static List<Vector3Int> GetSurrounding3x3Coordinates(this Vector3Int center, Vector3Int axisDirection)
    {
        var surrounding3x3Coordinates = new List<Vector3Int>();
        if (axisDirection.x != 0)
        {
            for (int y = -1; y < 2; y++)
            {
                for (int z = -1; z < 2; z++)
                {
                    surrounding3x3Coordinates.Add(new Vector3Int(center.x, center.y + y, center.z + z));
                }
            }
        }
        else if (axisDirection.y != 0)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int z = -1; z < 2; z++)
                {
                    surrounding3x3Coordinates.Add(new Vector3Int(center.x + x, center.y, center.z + z));
                }
            }
        }
        else if (axisDirection.z != 0)
        {
            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    surrounding3x3Coordinates.Add(new Vector3Int(center.x + x, center.y + y, center.z));
                }
            }
        }
        
        return surrounding3x3Coordinates;
    }

    public static Vector3Int GetWrappedNeg1To1(this Vector3Int v)
    {
        if (Math.Abs(v.x) > 2 || Math.Abs(v.y) > 2 || Math.Abs(v.z) > 2)
            throw new ArgumentOutOfRangeException($"Vector {v} has elements bigger than 2. This is will lead to unintended results.");
        
        return new Vector3Int(
            Math.Abs(v.x) > 1 ? -Math.Sign(v.x) : v.x,
            Math.Abs(v.y) > 1 ? -Math.Sign(v.y) : v.y,
            Math.Abs(v.z) > 1 ? -Math.Sign(v.z) : v.z
        );
    }
}