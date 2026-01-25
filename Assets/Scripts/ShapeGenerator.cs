using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class ShapeGenerator : MonoBehaviour
{
    public static ShapeGenerator Instance;

    private void Awake()
    {
        Instance = this;
    }

    public Shape GenerateCentered(Dictionary<Vector3Int, Building> cellDataByCoordinates)
    {
        var blockPositions = cellDataByCoordinates.Keys;
        var centerX = Mathf.Lerp(blockPositions.Min(pos => pos.x), blockPositions.Max(pos => pos.x), .5f);
        var centerY = Mathf.Lerp(blockPositions.Min(pos => pos.y), blockPositions.Max(pos => pos.y), .5f);
        var centerZ = Mathf.Lerp(blockPositions.Min(pos => pos.z), blockPositions.Max(pos => pos.z), .5f);
        var meshBoundCenter = new Vector3(centerX, centerY, centerZ) * Grid.CELL_SIZE;
        
        return Generate(cellDataByCoordinates, meshBoundCenter);
    }

    public Shape Generate(Dictionary<Vector3Int, Building> buildingByCoordinates, Vector3 customCenter = default)
    {
        var go = new GameObject
        {
            transform =
            {
                position = Vector3.zero,
                rotation = Quaternion.identity,
            }
        };
        var shapeObject = go.AddComponent<Shape>();
        
        foreach (var (coordinates, building) in buildingByCoordinates)
        {
            var position = shapeObject.transform.position + (Vector3)coordinates * Grid.CELL_SIZE - customCenter;
            var newBuilding = Instantiate(building, position, Quaternion.identity, shapeObject.transform);
            var cell = newBuilding.gameObject.GetComponent<Cell>();
            cell.meshRenderer.material = building.Material;
            shapeObject.CellsByCoordinate.Add(coordinates, cell);
        }
        return shapeObject;
    }
}