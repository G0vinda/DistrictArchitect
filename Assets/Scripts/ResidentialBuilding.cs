using UnityEngine;

public class ResidentialBuilding : Building
{
    [field: SerializeField] public int MaximumCapacity { get; private set; } = 5;

    [field: SerializeField] public int NumberOfResidents { get; set; }
    public bool IsAtFullCapacity => NumberOfResidents == MaximumCapacity;
}