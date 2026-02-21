using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class ShopBuilding : Building
{
    private const int BASE_FOOD = 5;
    private const int FOOD_INCREMENT = 2;
    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        var food = BASE_FOOD + 
                   position.Neighbours()
                       .Where(p => p.y == position.y 
                                   && Grid.HasCoordinate(p) 
                                   && grid.GetCellAt(p) != null 
                                   && grid.GetCellAt(p).Building is ResidentialBuilding)
                       .Sum(_ => FOOD_INCREMENT);

        return new PlacementResults(0, food, 0);
    }
}