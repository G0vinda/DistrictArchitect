using UnityEngine;

public class FoodBuilding : Building
{
    [field: SerializeField] public int MaxFoodSupply { get; private set; } = 3;

    [field: SerializeField] public int AmountOfFoodLeft { get; set; } = 3;
}