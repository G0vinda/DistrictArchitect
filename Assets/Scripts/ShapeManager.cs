using System.Collections.Generic;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    private List<List<Vector3Int>> _shapes = new();
    
    private void Awake()
    {
        var iShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(3, 0, 0) };
        var oShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(0, 1, 0), new(1, 1, 0) };
        var lShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 0, 0), new(0, 1, 0) };
        var tShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(-1, 0, 0), new(0, 1, 0) };
        var nShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(2, 1, 0), new(1, 1, 0) };
        var towerRightShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(1, 0, 1), new(1, 1, 1) };
        var towerLeftShape = new List<Vector3Int>() { new(0, 0, 0), new(1, 0, 0), new(0, 0, 1), new(0, 1, 1) };
        var tripodShape = new List<Vector3Int>() { new(0, 0, 0), new(0, 1, 0), new(0, 0, 1), new(1, 0, 0) };
        _shapes.Add(iShape);
        _shapes.Add(oShape);
        _shapes.Add(lShape);
        _shapes.Add(tShape);
        _shapes.Add(nShape);
        _shapes.Add(towerRightShape);
        _shapes.Add(towerLeftShape);
        _shapes.Add(tripodShape);
    }

    public List<Vector3Int> GetRandomShape()
    {
        return new List<Vector3Int>(_shapes[Random.Range(0, _shapes.Count)]);
    }
}