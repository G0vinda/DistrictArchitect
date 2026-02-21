using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class GreenhouseBuilding : Building
{
    private const int BASE_FOOD = 3;
    private const int FOOD_INCREMENT = 3;
    [SerializeField] private Building gardenBuilding;

    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        var producedFood = BASE_FOOD;

        var gardenShapeDefinition = new Dictionary<Vector3Int, Building>();
        foreach (var neighborPosition in position.Neighbours()
                     .Where(p => IsNeighborInPlaneEmptyAndSupported(position, p, grid)))
        {            
            producedFood += FOOD_INCREMENT;
            var relativePosition = neighborPosition - position;
            gardenShapeDefinition.Add(relativePosition, gardenBuilding);
        }

        var gardenShape = ShapeGenerator.Generate(gardenShapeDefinition);
        gardenShape.transform.position = Grid.GridCoordinatesToWorldPosition(position);
        
        var placementResults = new PlacementResults(0, producedFood, 0);
        placementResults.ExtraShapesToPlace.Add(gardenShape);
        
        return placementResults;
    }

    private static bool IsNeighborInPlaneEmptyAndSupported(Vector3Int origin, Vector3Int neighborPosition, Grid grid)
    {
        var supportPosition = neighborPosition + Vector3Int.down;

        return origin.y == neighborPosition.y
               && Grid.HasCoordinate(neighborPosition)
               && grid.GetCellAt(neighborPosition) == null
               && (neighborPosition.y == 0 
                   || (grid.GetCellAt(supportPosition) != null
                       && grid.GetCellAt(supportPosition).Building.isSupporting));
    }
}