using UnityEngine;

public class ResidentialBuilding : Building
{
    [field: SerializeField] public int MaximumCapacity { get; private set; } = 5;
    [SerializeField] private GameObject[] roofs;

    public int NumberOfResidents
    {
        get => _numberOfResidents;
        set
        {
            _numberOfResidents = value;
            for (int i = 0; i < roofs.Length; i++)
            {
                roofs[i].SetActive(i < _numberOfResidents);
            }
        }
    }
    
    public bool IsAtFullCapacity => _numberOfResidents == MaximumCapacity;
    
    private int _numberOfResidents;
}