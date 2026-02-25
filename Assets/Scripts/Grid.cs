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

    private const int PEOPLE_PER_FOOD = 5;
    private const int TAX_PER_LEVEL = 10;

    private int peopleCount = 0;
    private int foodCount = 10;
    private int moneyCount = 0;

    private int dayCount = 0;

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
        var isEmpty = area.All(coord => grid.ContainsKey(coord) && (grid[coord] == null || !grid[coord].Building.isBlocking));
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

    public bool PlaceShapeAtPosition(Shape shape, Vector3Int position, bool placementByPlayer = true)
    {
        HarvestFields(shape, position);
        AddShapeToGrid(shape, position);
        GetRewardsFromShape(shape, position);

        var gameFinished = false;
        if (placementByPlayer)
            gameFinished = AdvanceDay();
        
        return gameFinished;
    }

    private bool AdvanceDay()
    {
        dayCount++;
        
        var eatenFood = peopleCount / PEOPLE_PER_FOOD;
        eatenFood += peopleCount % PEOPLE_PER_FOOD != 0 ? 1 : 0;
        foodCount -= eatenFood;
        Debug.Log(peopleCount + " people eat " + eatenFood + " food!");

        if (dayCount % 7 == 0)
        {
            var highestLevel = GetHighestLevel();
            var taxes = highestLevel * TAX_PER_LEVEL;
            moneyCount -= taxes;
            Debug.Log("The City is " + highestLevel * 10 + " meters high and was taxed " + taxes + " money!");
        }

        Debug.Log("Day " + dayCount + "! People: " + peopleCount + ", Food: " + foodCount + ", Money: " + moneyCount);
        
        if (foodCount < 0 && moneyCount >= 0)
            Debug.Log("LOSS BY STARVATION");
        else if (moneyCount < 0 && foodCount >= 0)
            Debug.Log("LOSS BY BANKRUPTCY");
        else if (foodCount < 0)
            Debug.Log("LOSS BY BANKRUPT STARVATION");

        return foodCount < 0 || moneyCount < 0;
    }

    private void HarvestFields(Shape shape, Vector3Int position)
    {
        var countHarvestedFields = 0;
        foreach (var localCoordinate in shape.CellsByCoordinate.Keys)
        {
            var gridCoordinate = localCoordinate + position;
            if (grid[gridCoordinate] == null || grid[gridCoordinate].Building is not Field)
                continue;
            
            grid[gridCoordinate].DestroyWithVfx();
            countHarvestedFields++;
        }

        Debug.Log(countHarvestedFields + " fields harvested!");
        foodCount += countHarvestedFields;
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
                PlaceShapeAtPosition(extraShape, gridCoordinate, false);
        
            Debug.Log("The " + cell.Building.GetType() + " at " + gridCoordinate + " gave " + 
                      result.PeopleCount + " people, " + 
                      result.FoodCount + " food and " + 
                      result.MoneyCount + " money.");
        }
    }

    private void AddShapeToGrid(Shape shape, Vector3Int position)
    {
        foreach (var (localCoordinate, cell) in shape.CellsByCoordinate)
        {
            var gridCoordinate = localCoordinate + position;
            PlaceCellAt(cell, gridCoordinate);
        }
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

    private int GetHighestLevel()
    {
        return GetAllCellCoordinates().Max(p => p.y);
    }
}