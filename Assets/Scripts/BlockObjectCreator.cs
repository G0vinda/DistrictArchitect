using System.Collections.Generic;
using UnityEngine;

public class BlockObjectCreator : MonoBehaviour
{
    public List<UnityEngine.Vector3Int> blockPositions;
    public MeshFilter meshFilter;
    private int _vertexCount;
    
    void Start()
    {
        GenerateBlockObject(blockPositions);
    }

    public void GenerateBlockObject(List<UnityEngine.Vector3Int> blockPositions)
    {
        var mesh = new Mesh();
        mesh.vertices = new[] { Vector3.zero, Vector3.right, Vector3.up, new Vector3(1, 1, 0) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        meshFilter.mesh = mesh;
    }
}
