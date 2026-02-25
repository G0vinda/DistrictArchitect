using System.Collections.Generic;
using UnityEngine;
using WFC.TestScene;

namespace WFC
{
    public class TestWfcBlockCreator : MonoBehaviour
    {
        [SerializeField] private WfcConfig config;
        [SerializeField] private PlayerInput input;
        [SerializeField] private GameObject previewBlock;
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private GameObject errorSubBlockPrefab;
        [SerializeField] private BuildingSelection selection;

        private GameObject _spawnedBlock;
        
        private List<Vector3Int> _blockedBigPositions = new();
        private Dictionary<Vector3Int, GameObject> _subBlockGameObjectsBySmallCoordinate = new();
        private Dictionary<Vector3Int, int> _subBlockIdsBySmallCoordinate = new();
        private Vector3Int _lastHoveredCoordinate;

        private const float SUB_BLOCK_SIZE = 1f;

        private void OnEnable()
        {
            input.OnMouseClicked += BuildNewBlock;
        }
        
        private void OnDisable()
        {
            input.OnMouseClicked -= BuildNewBlock;
        }

        private void Update()
        {
            var ray = Camera.main.ScreenPointToRay(input.MousePosition);
            if (Physics.Raycast(ray, out var hit))
            {
                var hitPoint = hit.point + hit.normal * 0.005f;
                var hoveredCoordinate = WorldPositionToBigCoordinate(hitPoint);
                if (hoveredCoordinate != _lastHoveredCoordinate)
                {
                    Debug.Log($"hit at {hit.point} -> {hoveredCoordinate}");
                    _lastHoveredCoordinate = hoveredCoordinate;
                    previewBlock.transform.position = BigCoordinateToWorldPosition(_lastHoveredCoordinate);
                    previewBlock.SetActive(true);
                }
            }
            else
            {
                previewBlock.SetActive(false);
            }
        }

        private void BuildNewBlock()
        {
            var ray = Camera.main.ScreenPointToRay(input.MousePosition);
            if (!Physics.Raycast(ray, out var hit))
                return;
            
            var hitPoint = hit.point + hit.normal * 0.005f;
            var bigCoordinate = WorldPositionToBigCoordinate(hitPoint);
            Debug.Log($"Trying to place Block at hit at {hitPoint} -> {bigCoordinate}");
            
            var newBlock = Instantiate(blockPrefab, BigCoordinateToWorldPosition(bigCoordinate), Quaternion.identity);
            _blockedBigPositions.Add(bigCoordinate);

            var possibleSubBlockIdsByCoordinates = new Dictionary<Vector3Int, int[]>();

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    for (int z = -1; z < 2; z++)
                    {
                        var localSubBlockCoordinate = new Vector3Int(x, y, z);
                        var globalSubBlockCoordinate = GetGlobalSmallCoordinateFromLocal(localSubBlockCoordinate, bigCoordinate);
                        
                    }
                }   
            }
        }

        private static Vector3Int WorldPositionToBigCoordinate(Vector3 worldPosition)
        {
            var dividedPosition = worldPosition / (SUB_BLOCK_SIZE * 3f);
            return new Vector3Int(
                Mathf.RoundToInt(dividedPosition.x), 
                Mathf.RoundToInt(dividedPosition.y), 
                Mathf.RoundToInt(dividedPosition.z));
        }

        private static Vector3 BigCoordinateToWorldPosition(Vector3Int bigCoordinate)
        {
            return (Vector3)bigCoordinate * (SUB_BLOCK_SIZE * 3f);
        }

        private static Vector3Int GetGlobalSmallCoordinateFromLocal(Vector3Int localCoordinate,
            Vector3Int bigCoordinate)
        {
            return bigCoordinate * 3 + localCoordinate;
        }
    }
}