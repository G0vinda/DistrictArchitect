using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;

public class FactoryBuilding : Building
{
    private const int BASE_INCOME = 10;
    private const int INCOME_INCREMENT = 1;
    
    private const int BASE_SMOKE_STRENGTH = 1;
    
    private const int SMOKE_DROP_OFF_DISTANCE = 2;
    private const int SMOKE_DROP_OFF = 1;
    private const int MOVE_OUT_PER_SMOKE = 1;

    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        //INCOME CALCULATION
        var cluster = grid.FindCellCluster(position);
        var clusterSize = cluster.Count - 1;
        var income = BASE_INCOME + clusterSize * INCOME_INCREMENT;

        var factoryOrigin = position;

        var index = cluster.FindIndex(p => grid.SmokePointsByFactoryOrigin.ContainsKey(p));
        if (index != -1)
            factoryOrigin = cluster[index];
        
        //either update smokeHeight or strength depending on the new block in the factory
        if (grid.SmokePointsByFactoryOrigin.ContainsKey(factoryOrigin))
        {
            var smokePoint = grid.SmokePointsByFactoryOrigin[factoryOrigin];
            
            if (position.y > smokePoint.Position.y)
                smokePoint.Position = position;
            else
                smokePoint.Strength += 1;

            grid.SmokePointsByFactoryOrigin[factoryOrigin] = smokePoint;
        }
        else //create a new basic smoke
            grid.SmokePointsByFactoryOrigin.Add(factoryOrigin, new SmokeOrigin(position, BASE_SMOKE_STRENGTH));
        
        var updatedSmokePoint = grid.SmokePointsByFactoryOrigin[factoryOrigin];
        var smokePosition = updatedSmokePoint.Position;
        
        //lower PeopleCount for every living space in smoke area,
        //closer areas will be influenced by this more often while the factory is being constructed
        var moveOuts = 0;
        for (var x = 0; x < Grid.MAP_SIZE; x++)
        {
            for (var y = smokePosition.y + 1; y < Grid.MAP_SIZE; y++)
            {
                for (var z = 0; z < Grid.MAP_SIZE; z++)
                {
                    var currentPos = new Vector3Int(x, y, z);
                    var cell = grid.GetCellAt(currentPos);
                    if (cell == null || cell.Building is not ResidentialBuilding)
                        continue;

                    var strength = GetSmokeStrengthForSmokeOriginAtPosition(updatedSmokePoint, currentPos);
                    
                    if (strength > 0)
                        moveOuts -= MOVE_OUT_PER_SMOKE;
                }
            }
        }

        return new PlacementResults(moveOuts, 0, income);
    }

    public static int GetSmokeStrengthForSmokeOriginAtPosition(SmokeOrigin smokeOrigin, Vector3Int position)
    {
        var smokePosition = smokeOrigin.Position;
        if (smokePosition.y >= position.y) return 0;
        
        var distance = Math.Min(Math.Abs(smokePosition.x - position.x), Math.Abs(smokePosition.z - position.z));
        var dropOff = distance / SMOKE_DROP_OFF_DISTANCE * SMOKE_DROP_OFF;
        return Math.Max(0, smokeOrigin.Strength - dropOff);
    }
}

public struct SmokeOrigin
{
    public Vector3Int Position;
    public int Strength;

    public SmokeOrigin(Vector3Int position, int strength)
    {
        Position = position;
        Strength = strength;
    }
}