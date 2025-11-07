using System;

[Serializable]
public struct Vector3Int : IEquatable<Vector3Int>
{
    public int x;
    public int y;
    public int z;

    public Vector3Int(int x, int y, int z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }
    
    public static Vector3Int operator + (Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.x + b.x, a.y + b.y, a.z + b.z);
    }
    
    public static Vector3Int operator - (Vector3Int a, Vector3Int b)
    {
        return new Vector3Int(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static Vector3Int operator *(Vector3Int vec, int factor)
    {
        return new Vector3Int(vec.x * factor, vec.y * factor, vec.z * factor);
    }

    public static bool operator ==(Vector3Int a, Vector3Int b)
    {
        return a.x == b.x && a.y == b.y && a.z == b.z;
    }

    public static bool operator !=(Vector3Int a, Vector3Int b)
    {
        return !(a == b);
    }

    public override string ToString()
    {
        return $"({x}, {y}, {z})";
    }

    public bool Equals(Vector3Int other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is Vector3Int other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, y, z);
    }
}