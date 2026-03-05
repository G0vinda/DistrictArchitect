using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Garden : Building
{
    public override PlacementResults GetPlacementReward(Grid grid, Vector3Int position)
    {
        return new PlacementResults(0, 0, 0);
    }
}