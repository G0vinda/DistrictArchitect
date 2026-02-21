using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [field: SerializeField] public Material Material { get; private set; }
    
    public abstract PlacementResults GetPlacementReward(Grid grid, Vector3Int position);

    
}