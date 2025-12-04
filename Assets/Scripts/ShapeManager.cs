using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeManager : MonoBehaviour
{
    private List<List<Vector3Int>> _shapes = new();

    [SerializeField] private CellData[] cellData;
    
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

    public Dictionary<Vector3Int, CellData> GetRandomShapeDefinition()
    {
        var cellDataList = cellData.ToList();
        var cellData1 = cellDataList[Random.Range(0, cellDataList.Count)];
        cellDataList.Remove(cellData1);
        var cellData2 = cellDataList[Random.Range(0, cellDataList.Count)];
        var cellCoordinates = new List<Vector3Int>(_shapes[Random.Range(0, _shapes.Count)]);
        
        var cellDataByCoordinates = new Dictionary<Vector3Int, CellData>();
        var coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
        cellCoordinates.Remove(coords);
        cellDataByCoordinates.Add(coords, cellData1);
        coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
        cellCoordinates.Remove(coords);
        cellDataByCoordinates.Add(coords, cellData2);

        while (cellCoordinates.Count > 0)
        {
            coords = cellCoordinates[Random.Range(0, cellCoordinates.Count)];
            cellCoordinates.Remove(coords);
            cellDataByCoordinates.Add(coords, Random.Range(0, 2) == 0 ? cellData1 : cellData2);
        }

        return cellDataByCoordinates;
    }
}