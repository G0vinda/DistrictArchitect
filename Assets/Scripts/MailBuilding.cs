using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MailBuilding : Building
{
    private const int BASE_INCOME = 0;
    private const int INCOME_INCREMENT = 2;

    private readonly Vector3Int[] directions = { Vector3Int.right, Vector3Int.left, Vector3Int.forward, 
        Vector3Int.back, Vector3Int.up, Vector3Int.down };

    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        var income = BASE_INCOME;
        foreach (var direction in directions)
        {
            var incomeOfDirection = 0;
            var next = position + direction;
            while (Grid.HasCoordinate(next))
            {
                var cell = grid.GetCellAt(next);
                
                next += direction;

                if (cell == null) 
                    continue;
                
                if (cell.Building is MailBuilding)
                {
                    income += incomeOfDirection;
                    break;
                }
                
                if (cell.Building is ResidentialBuilding) 
                    incomeOfDirection += INCOME_INCREMENT;
            }
        }

        return new PlacementResults(0, 0, income);
    }
}