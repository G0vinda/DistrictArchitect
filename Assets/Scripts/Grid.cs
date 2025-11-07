using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
{
    private Dictionary<Vector3Int, bool> grid = new();

    public const int MAP_SIZE = 6;
    public const float CELL_SIZE = 1f;
    public const float BLOCK_OFFSET = 0.5f;
    
    private const float Y_BUFFER = 0.05f;

    public Grid()
    {
        for (var x = 0; x < MAP_SIZE; x++)
        {
            for (var z = 0; z < MAP_SIZE; z++)
            {
                grid.Add(new Vector3Int(x, 0, z), false);
            }
        }
    }
    
    public bool IsEmptyAtArea(IEnumerable<Vector3Int> area) => area.All(coord => grid.ContainsKey(coord) && !grid[coord]);
    
    public bool IsEmptyAt(Vector3Int coordinates) => !grid[coordinates];
    
    public bool PlaceAtArea(IEnumerable<Vector3Int> area) => area.All(coord => grid[coord] = true); 
    
    public void PlaceAt(Vector3Int coordinates) => grid[coordinates] = true;
    
    public static Vector3Int WorldPositionToGridCoordinates(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / CELL_SIZE),
            Mathf.FloorToInt(position.y + Y_BUFFER / CELL_SIZE),
            Mathf.FloorToInt(position.z / CELL_SIZE));
    }

    public static Vector3 GridCoordinatesToWorldPosition(Vector3Int coordinates)
    {
        return new Vector3(
            coordinates.x * CELL_SIZE + BLOCK_OFFSET,
            coordinates.y * CELL_SIZE + BLOCK_OFFSET,
            coordinates.z * CELL_SIZE + BLOCK_OFFSET);
    }
}