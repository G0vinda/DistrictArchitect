using System.Linq;
using TMPro;
using Unity.Mathematics.Geometry;
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
        playerInput.OnVerticalTurn += TurnPreviewVertically;
        playerInput.OnHorizontalTurn += TurnPreviewHorizontally;
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
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point + 0.1f * hit.normal);
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

    private void TurnPreviewHorizontally(float turnDirection)
    {
        if (!blockPreview.activeSelf) return;

        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        Debug.Log(turnAngle);

        blockPreview.transform.Rotate(Vector3.up, turnAngle);
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!blockPreview.activeSelf) return;
        
        var turnAngle = Mathf.Sign(turnDirection) * 90f;

        var axis = cam.transform.forward.z > cam.transform.forward.x ? Vector3.right : Vector3.forward;
        blockPreview.transform.Rotate(axis, turnAngle);
    }
}