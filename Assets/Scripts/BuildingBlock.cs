using System.Collections.Generic;
using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    [field: SerializeField] public List<Vector3Int> Positions { get; private set; } 
    [field: SerializeField] public GameObject PreviewPrefab { get; private set; }
}
