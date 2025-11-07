using System;
using System.Linq;
using TMPro;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] BuildingBlock blockPrefab;
    [SerializeField] TextMeshProUGUI debugHoverText;
    
    private Grid grid;
    private Camera cam;
    private GameObject blockPreview;
    private Vector3Int? lastHoveredGridCoordinates = null;
    
    private void Start()
    {
        grid = new Grid();
        cam = Camera.main;
        blockPreview = Instantiate(blockPrefab.PreviewPrefab);
        blockPreview.SetActive(false);
    }

    private void OnEnable()
    {
        playerInput.OnMousePositionChanged += CheckForEmptyBuildingSlot;
        playerInput.OnMouseClicked += TryPlaceBuilding;
    }

    private void OnDisable()
    {
        playerInput.OnMousePositionChanged -= CheckForEmptyBuildingSlot;
        playerInput.OnMouseClicked -= TryPlaceBuilding;
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        var mouseRay = cam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(mouseRay, out var hit))
        {
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point);
            if (lastHoveredGridCoordinates != null && hitGridCoordinates == lastHoveredGridCoordinates.Value)
                return;
            
            debugHoverText.text = $"Hovering:{hitGridCoordinates}";
            lastHoveredGridCoordinates = hitGridCoordinates;
            var hitArea = blockPrefab.Positions.Select(pos => pos + hitGridCoordinates).ToList();
            if (grid.IsEmptyAtArea(hitArea))
            {
                blockPreview.transform.position = Grid.GridCoordinatesToWorldPosition(hitGridCoordinates);
                blockPreview.SetActive(true);
                return;
            }
        }
        blockPreview.SetActive(false);
    }

    private void TryPlaceBuilding()
    {
        var mouseRay = cam.ScreenPointToRay(playerInput.MousePosition);
        if (Physics.Raycast(mouseRay, out var hit))
        {
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point);
            var hitArea = blockPrefab.Positions.Select(pos => pos + hitGridCoordinates);
            Debug.Log($"Hit at grid coordinates: {hitGridCoordinates}");
            if (grid.IsEmptyAtArea(hitArea))
            {
                var newBlockPosition = Grid.GridCoordinatesToWorldPosition(hitGridCoordinates);
                Instantiate(blockPrefab, newBlockPosition, Quaternion.identity);
                grid.PlaceAtArea(hitArea);
            }
        }
    }
}