using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class Grid
{
    private readonly Dictionary<Vector3Int, Cell> grid = new();

    public const int MAP_SIZE = 5;
    public const float CELL_SIZE = 1f;
    
    private const float BLOCK_OFFSET = 0.5f;
    private const int GAME_OVER_HEIGHT = 5;
    
    private int peopleCount;
    private int foodCount;
    private int moneyCount;

    public readonly Dictionary<Vector3Int, SmokeOrigin> SmokePointsByFactoryOrigin = new();
    
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
        var hasGround = area.Any(coord =>
            coord.y == 0 || grid.ContainsKey(coord + Vector3Int.down) && grid[coord + Vector3Int.down] != null);
        return isEmpty && hasGround;
    }

    public static bool HasCoordinate(Vector3Int coordinate)
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

    public bool PlaceShapeAtPosition(Shape shape, Vector3Int position)
    {
        var doesPlacementFinishGame = AddShapeToGrid(shape, position);

        GetRewardsFromShape(shape, position);
        
        Debug.Log("New People Count: " + peopleCount + ", New Food Count: " + foodCount + ", New Money Count: " + moneyCount);

        return doesPlacementFinishGame;
    }

    private void GetRewardsFromShape(Shape shape, Vector3Int position)
    {
        foreach (var (localCoordinate, cell) in shape.CellsByCoordinate)
        {
            var gridCoordinate = localCoordinate + position;
            var result = cell.Building.GetPlacementReward(this, gridCoordinate);
            peopleCount += result.PeopleCount;
            foodCount += result.FoodCount;
            moneyCount += result.MoneyCount;
        
            foreach (var extraShape in result.ExtraShapesToPlace)
                PlaceShapeAtPosition(extraShape, gridCoordinate);
        
            Debug.Log("The " + cell.Building.GetType() + " at " + gridCoordinate + " gave " + 
                      result.PeopleCount + " people, " + 
                      result.FoodCount + " food and " + 
                      result.MoneyCount + " money.");
        }
    }

    private bool AddShapeToGrid(Shape shape, Vector3Int position)
    {
        var doesPlacementFinishGame = false;
        foreach (var (localCoordinate, cell) in shape.CellsByCoordinate)
        {
            var gridCoordinate = localCoordinate + position;
            if (gridCoordinate.y >= (GAME_OVER_HEIGHT - 1)) doesPlacementFinishGame = true;
            PlaceCellAt(cell, gridCoordinate);
        }

        return doesPlacementFinishGame;
    }

    public void PlaceCellAt(Cell cell, Vector3Int gridCoordinate)
    {
        grid[gridCoordinate] = cell;
    }


    public List<Cell> GetAllCellObjects()
    {
        return grid.Values.Where(cell => cell != null).ToList();
    }

    public List<Vector3Int> GetAllCellCoordinates()
    {
        return grid.Keys.Where(coord => grid[coord] != null).ToList();
    }

    public Cell GetCellAt(Vector3Int coordinates)
    {
        return grid[coordinates];
    }

    public List<Vector3Int> FindCellCluster(Vector3Int startCoordinate)
    {
        if (grid[startCoordinate] == null)
            return new List<Vector3Int>();
        
        var searchQueue = new Queue<Vector3Int>(startCoordinate.Neighbours());
        var visited = new HashSet<Vector3Int>() { startCoordinate };
        var clusterCoordinates = new List<Vector3Int>() { startCoordinate };
        var clusteredBuilding = GetCellAt(startCoordinate).Building.GetType();
        
        while (searchQueue.Count > 0)
        {
            var searchCoordinate = searchQueue.Dequeue();
            var isNewCoordinate = visited.Add(searchCoordinate);
            
            if (!HasCoordinate(searchCoordinate)
                || !isNewCoordinate
                || GetCellAt(searchCoordinate) == null
                || GetCellAt(searchCoordinate).Building.GetType() != clusteredBuilding)
                continue;

            foreach (var neighbor in searchCoordinate.Neighbours())
                searchQueue.Enqueue(neighbor);

            clusterCoordinates.Add(searchCoordinate);
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
                case 0:
                    row.Add(new Vector3Int(i, rowConstant.x, rowConstant.y));
                    break;
                case 1:
                    row.Add(new Vector3Int(rowConstant.x, i, rowConstant.y));
                    break;
                case 2:
                    row.Add(new Vector3Int(rowConstant.x, rowConstant.y, i));
                    break;
            }
        }

        return row;
    }
}