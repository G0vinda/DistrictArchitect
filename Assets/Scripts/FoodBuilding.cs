using UnityEngine;

public class FoodBuilding : Building
{
    [field: SerializeField] public int MaxFoodSupply { get; private set; } = 3;
    [SerializeField] private GameObject[] trees;

    [field: SerializeField]
    public int AmountOfFoodLeft
    {
        get => _amountOfFoodLeft;
        set
        {
            for (int i = 0; i < trees.Length; i++)
            {
                trees[i].SetActive(i < value);
            }
            _amountOfFoodLeft = value;  
        } 
    }

    private int _amountOfFoodLeft = 3;
}