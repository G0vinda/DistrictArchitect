using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingBlock : MonoBehaviour
{
    [field: SerializeField] public List<Vector3Int> Positions { get; private set; } 
    [field: SerializeField] public GameObject PreviewPrefab { get; private set; }
}
