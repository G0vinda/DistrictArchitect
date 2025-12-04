using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class BlockMeshGenerator
{
    public const float BLOCK_SIZE = 1f;
    public const float HALF_BLOCK_SIZE = BLOCK_SIZE / 2f;

    public static Mesh GenerateCenteredMesh(List<Vector3Int> blockPositions)
    {
        var centerX = Mathf.Lerp(blockPositions.Min(pos => pos.x), blockPositions.Max(pos => pos.x), .5f);
        var centerY = Mathf.Lerp(blockPositions.Min(pos => pos.y), blockPositions.Max(pos => pos.y), .5f);
        var centerZ = Mathf.Lerp(blockPositions.Min(pos => pos.z), blockPositions.Max(pos => pos.z), .5f);
        var meshBoundCenter = new Vector3(centerX, centerY, centerZ) * BLOCK_SIZE;
        
        return Generate(blockPositions, meshBoundCenter);
    }
    
    public static Mesh Generate(List<Vector3Int> blockPositions, Vector3 customCenter = default)
    {
        List<Vector3> vertices = new();
        List<int> triangles = new();
        List<Vector3> normals = new();
        var vCount = 0;
        
        foreach (var blockPosition in blockPositions)
        {
            if (!blockPositions.Contains(blockPosition + Vector3Int.down))
                CreateDownFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
            if (!blockPositions.Contains(blockPosition + Vector3Int.up))
                CreateUpFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
            if (!blockPositions.Contains(blockPosition + Vector3Int.forward))
                CreateFrontFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
            if (!blockPositions.Contains(blockPosition + Vector3Int.back))
                CreateBackFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
            if (!blockPositions.Contains(blockPosition + Vector3Int.left))
                CreateLeftFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
            if (!blockPositions.Contains(blockPosition + Vector3Int.right))
                CreateRightFace(blockPosition, ref vCount, customCenter, vertices, triangles, normals);
        }
        
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private static void CreateDownFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 2, vCount + 1, vCount, vCount + 3, vCount + 2 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.down); }
        vCount += 4;
    }
    
    private static void CreateUpFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 2, vCount + 1, vCount, vCount + 3, vCount + 2 });
        triangles.AddRange(new List<int>(){ vCount, vCount + 1, vCount + 2, vCount, vCount + 2, vCount + 3 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.up); }
        vCount += 4;
    }

    private static void CreateBackFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 1, vCount + 2, vCount, vCount + 2, vCount + 3 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.back); }
        vCount += 4;
    }
    
    private static void CreateFrontFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 2, vCount + 1, vCount, vCount + 3, vCount + 2 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.forward); }
        vCount += 4;
    }

    private static void CreateLeftFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(-HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 1, vCount + 2, vCount, vCount + 2, vCount + 3 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.left); }
        vCount += 4;
    }

    private static void CreateRightFace(Vector3Int blockPosition, ref int vCount, Vector3 center, List<Vector3> vertices, List<int> triangles, List<Vector3> normals)
    {
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        vertices.Add((Vector3)blockPosition * BLOCK_SIZE - center + new Vector3(HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE, -HALF_BLOCK_SIZE));
        triangles.AddRange(new List<int>(){ vCount, vCount + 2, vCount + 1, vCount, vCount + 3, vCount + 2 });
        for (int i = 0; i < 4; i++) { normals.Add(Vector3.right); }
        vCount += 4;
    }
}
