using System;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public abstract class Building : MonoBehaviour
{
    [field: SerializeField] public Material Material { get; private set; }
    [field: SerializeField] public bool isBlocking { get; private set; } = true;
    [field: SerializeField] public bool isSupporting { get; private set; } = true;
    
    public abstract PlacementResults GetPlacementReward(Grid grid, Vector3Int position);
}