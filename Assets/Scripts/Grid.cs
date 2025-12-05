using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid
{
    private readonly Dictionary<Vector3Int, CellObject> grid = new();

    public const int MAP_SIZE = 6;
    public const float CELL_SIZE = 1f;
    public const float BLOCK_OFFSET = 0.5f;
    public const int GAME_OVER_HEIGHT = 5;

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
        var hasGround = area.Any(coord => coord.y == 0 || grid.ContainsKey(coord + Vector3Int.down) && grid[coord + Vector3Int.down] != null);
        return isEmpty && hasGround;
    }

    public static bool IsCoordinateInGrid(Vector3Int coordinate)
    {
        return coordinate is { x: >= 0 and < MAP_SIZE, y: >= 0 and < MAP_SIZE, z: >= 0 and < MAP_SIZE };
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

    public bool PlaceShapeAtPosition(ShapeObject shape, Vector3Int position)
    {
        var doesPlacementFinishGame = false;
        foreach (var (localCoordinate, cell) in shape.CellsByCoordinate)
        {
            var gridCoordinate = localCoordinate + position;
            if (gridCoordinate.y >= (GAME_OVER_HEIGHT-1)) doesPlacementFinishGame = true;
            grid[gridCoordinate] = cell;
        }
        return doesPlacementFinishGame;
    }

    public List<CellObject> GetAllCellObjects()
    {
        return grid.Values.Where(cell => cell != null).ToList();
    }

    public List<Vector3Int> GetAllCellCoordinates()
    {
        return grid.Keys.Where(coord => grid[coord] != null).ToList();
    }

    public CellObject GetCellObjectAtCoordinates(Vector3Int coordinates)
    {
        return grid[coordinates];
    }

    public List<Vector3Int> FindCellCluster(Vector3Int startCoordinate)
    {
        if (grid[startCoordinate] == null) 
            return new List<Vector3Int>();
        
        var openCoordinates = new Queue<Vector3Int>();
        openCoordinates.Enqueue(startCoordinate);
        var closedCoordinates = new HashSet<Vector3Int>() {startCoordinate};
        var clusterCoordinates = new List<Vector3Int>() {startCoordinate};
        var directions = new[]
            { Vector3Int.up, Vector3Int.down, Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back };
        var clusterData = grid[startCoordinate].CellData;

        while (openCoordinates.Count > 0)
        {
            var current = openCoordinates.Dequeue();
            foreach (var dir in directions)
            {
                var neighbor = current + dir;
                if (!closedCoordinates.Add(neighbor))
                    continue;

                if (!IsCoordinateInGrid(neighbor))
                    continue;
                
                if (grid[neighbor] == null || grid[neighbor].CellData != clusterData)
                    continue;
                
                clusterCoordinates.Add(neighbor);
                openCoordinates.Enqueue(neighbor);
            }
        }
        return clusterCoordinates;
    }

    public List<Vector3Int> GetAllGridCoordinates()
    {
        return grid.Keys.ToList();
    }

    public List<Vector3Int> GetRow(Vector2Int rowConstant, int dimension)
    {
        var row = new List<Vector3Int>();

        for (var i = 0; i < MAP_SIZE; i++)
        {
            switch (dimension)
            {
                case 0: row.Add(new Vector3Int(i, rowConstant.x, rowConstant.y));
                    break;
                case 1: row.Add(new Vector3Int(rowConstant.x, i, rowConstant.y));
                    break;
                case 2: row.Add(new Vector3Int(rowConstant.x, rowConstant.y, i));
                    break;
            }
        }

        return row;
    }
}