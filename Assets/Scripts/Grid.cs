using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;

public class Grid
{
    private Dictionary<Vector3Int, CellObject> grid = new();

    public const int MAP_SIZE = 6;
    public const float CELL_SIZE = 1f;
    public const float BLOCK_OFFSET = 0.5f;

    public Grid()
    {
        for (var x = 0; x < MAP_SIZE; x++)
        {
            for (var y = 0; y < MAP_SIZE; y++)
            {
                for (var z = 0; z < MAP_SIZE; z++)
                {
                    grid.Add(new Vector3Int(x, y, z), null);
                }   
            }
        }
    }

    public bool CanShapeBePlacedAtArea(IEnumerable<Vector3Int> area)
    {
        var isEmpty = area.All(coord => grid.ContainsKey(coord) && grid[coord] == null);
        var hasGround = area.Any(coord => coord.y == 0 || grid.ContainsKey(coord + Vector3Int.down) && grid[coord + Vector3Int.down] == null);
        return isEmpty && hasGround;
    }

    public static bool IsCoordinateInGrid(Vector3Int coordinate)
    {
        return coordinate is { x: >= 0 and < MAP_SIZE, z: >= 0 and < MAP_SIZE };
    }
    
    public static Vector3Int WorldPositionToGridCoordinates(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / CELL_SIZE),
            Mathf.FloorToInt(position.y / CELL_SIZE),
            Mathf.FloorToInt(position.z / CELL_SIZE));
    }

    public static Vector3 GridCoordinatesToWorldPosition(Vector3Int coordinates)
    {
        return new Vector3(
            coordinates.x * CELL_SIZE + BLOCK_OFFSET,
            coordinates.y * CELL_SIZE + BLOCK_OFFSET,
            coordinates.z * CELL_SIZE + BLOCK_OFFSET);
    }

    public void PlaceShapeAtPosition(ShapeObject shape, Vector3Int position)
    {
        foreach (var (localCoordinate, cell) in shape.CellsByCoordinate)
        {
            grid[localCoordinate + position] = cell;
        }
    }
}