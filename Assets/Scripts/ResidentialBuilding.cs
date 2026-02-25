using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class ResidentialBuilding : Building
{
    private const int BASE_RESIDENCY = 5;
    private const int RESIDENCY_INCREMENT = 1;
    private const int MAX_CLUSTER_INCREMENT = 5;
    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        var cluster = grid.FindCellCluster(position);
        var newResidents = BASE_RESIDENCY;
        foreach (var smokePoint in grid.SmokePointsByFactoryOrigin.Values
                     .Where(smokeOrigin => smokeOrigin.Position.y < position.y))
        {
            var strength = FactoryBuilding.GetSmokeStrengthForSmokeOriginAtPosition(smokePoint, position);
            newResidents -= strength;
        }

        newResidents += Math.Min(cluster.Count - 1, MAX_CLUSTER_INCREMENT) * RESIDENCY_INCREMENT;
        
        return new PlacementResults(Math.Max(newResidents, 0), 0, 0);
    }
}