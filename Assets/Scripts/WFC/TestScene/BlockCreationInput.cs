using System;
using UnityEngine;

namespace WFC.TestScene
{
    public class BlockCreationInput : MonoBehaviour
    {
        [SerializeField] private PlayerInput input;
        [SerializeField] private GameObject previewBlock;
        [SerializeField] private BuildingSelection selection;
        [SerializeField] private WfcBlockCreator blockCreator;
        [SerializeField] private Transform groundTransform;

        private Vector3Int _lastHoveredCoordinate;

        private void Awake()
        {
            previewBlock.transform.localScale = Vector3.one * 3f * blockCreator.Config.subBlockSize;
            var groundPosition = groundTransform.position;
            groundPosition.y = - 1.5f * blockCreator.Config.subBlockSize - 0.5f;
            groundTransform.position = groundPosition;
        }

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
        
        private Vector3Int WorldPositionToBigCoordinate(Vector3 worldPosition)
        {
            var dividedPosition = worldPosition / (blockCreator.Config.subBlockSize * 3f);
            return new Vector3Int(
                Mathf.RoundToInt(dividedPosition.x), 
                Mathf.RoundToInt(dividedPosition.y), 
                Mathf.RoundToInt(dividedPosition.z));
        }
        
        private Vector3 BigCoordinateToWorldPosition(Vector3Int bigCoordinate)
        {
            return (Vector3)bigCoordinate * (blockCreator.Config.subBlockSize * 3f);
        }
    }
}