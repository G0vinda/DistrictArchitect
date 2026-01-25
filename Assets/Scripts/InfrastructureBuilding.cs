using UnityEngine;

public class InfrastructureBuilding : Building
{
    [field: SerializeField] public int Connectivity { get; private set; } = 100;
}