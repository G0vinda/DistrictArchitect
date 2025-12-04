using UnityEngine;

[CreateAssetMenu(fileName = "CellData", menuName = "CellData")]
public class CellData : ScriptableObject
{
    [field: SerializeField] public Material Material { get; private set; }
}