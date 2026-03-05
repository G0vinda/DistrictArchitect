using UnityEngine;

namespace WFC.TestScene
{
    public class BlockCreationInput : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        [SerializeField] private GameObject previewBlock;
        [SerializeField] private BuildingSelection selection;
        [SerializeField] private WfcBlockCreator blockCreator;

        private Vector3Int _lastHoveredCoordinate;

        private const float SUB_BLOCK_SIZE = 1f;
        
        private void OnEnable()
        {
            input.OnMouseClicked += HandleMouseClicked;
        }

        private void OnDisable()
        {
            input.OnMouseClicked -= HandleMouseClicked;
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
        
        private void HandleMouseClicked()
        {
            var ray = Camera.main.ScreenPointToRay(input.MousePosition);
            if (!Physics.Raycast(ray, out var hit))
                return;
            
            var hitPoint = hit.point + hit.normal * 0.005f;
            var bigCoordinate = WorldPositionToBigCoordinate(hitPoint);
            
            blockCreator.BuildNewBlock(bigCoordinate);
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
    }
}