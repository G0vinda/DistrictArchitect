using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShapeObjectGenerator : MonoBehaviour
{
    [SerializeField] private CellObject cellObjectPrefab;
    
    public static ShapeObjectGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public ShapeObject GenerateCentered(Dictionary<Vector3Int, CellData> cellDataByCoordinates)
    {
        var blockPositions = cellDataByCoordinates.Keys;
        var centerX = Mathf.Lerp(blockPositions.Min(pos => pos.x), blockPositions.Max(pos => pos.x), .5f);
        var centerY = Mathf.Lerp(blockPositions.Min(pos => pos.y), blockPositions.Max(pos => pos.y), .5f);
        var centerZ = Mathf.Lerp(blockPositions.Min(pos => pos.z), blockPositions.Max(pos => pos.z), .5f);
        var meshBoundCenter = new Vector3(centerX, centerY, centerZ) * Grid.CELL_SIZE;
        
        return Generate(cellDataByCoordinates, meshBoundCenter);
    }

    public ShapeObject Generate(Dictionary<Vector3Int, CellData> cellDataByCoordinates, Vector3 customCenter = default)
    {
        var go = new GameObject
        {
            transform =
            {
                position = Vector3.zero,
                rotation = Quaternion.identity,
            }
        };
        var shapeObject = go.AddComponent<ShapeObject>();
        
        foreach (var (coordinates, cellData) in cellDataByCoordinates)
        {
            var position = shapeObject.transform.position + (Vector3)coordinates * Grid.CELL_SIZE - customCenter;
            var newCellObject = Instantiate(cellObjectPrefab, position, Quaternion.identity, shapeObject.transform);
            newCellObject.CellData = cellData;
            shapeObject.CellsByCoordinate.Add(coordinates, newCellObject);
        }
        return shapeObject;
    }
}