using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] BuildingBlock blockPrefab;
    [SerializeField] TextMeshProUGUI debugHoverText;
    
    private Grid _grid;
    private Camera _cam;
    private GameObject _blockPreview;
    private List<Vector3Int> _rotatedBlockPositions;
    private Vector3Int? _lastHoveredGridCoordinates = null;
    
    private void Start()
    {
        _grid = new Grid();
        _cam = Camera.main;
        SelectBlock(blockPrefab);
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

    public void SelectBlock(BuildingBlock block)
    {
        if (_blockPreview != null)
            Destroy(_blockPreview);
        
        _blockPreview = Instantiate(block.PreviewPrefab);
        _blockPreview.SetActive(false);
        _rotatedBlockPositions = new List<Vector3Int>(blockPrefab.Positions);
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(mouseRay, out var hit))
        {
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point + 0.1f * hit.normal);
            if (_lastHoveredGridCoordinates != null && hitGridCoordinates == _lastHoveredGridCoordinates.Value)
                return;
            
            debugHoverText.text = $"Hovering:{hitGridCoordinates}";
            _lastHoveredGridCoordinates = hitGridCoordinates;
            var hitArea = _rotatedBlockPositions.Select(pos => pos + hitGridCoordinates).ToList();
            if (_grid.IsEmptyAtArea(hitArea))
            {
                _blockPreview.transform.position = Grid.GridCoordinatesToWorldPosition(hitGridCoordinates);
                _blockPreview.SetActive(true);
                return;
            }
        }
        else
        {
            _lastHoveredGridCoordinates = null;
        }
        _blockPreview.SetActive(false);
    }

    private void TryPlaceBuilding()
    {
        var mouseRay = _cam.ScreenPointToRay(playerInput.MousePosition);
        if (Physics.Raycast(mouseRay, out var hit))
        {
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point);
            var hitArea = _rotatedBlockPositions.Select(pos => pos + hitGridCoordinates).ToList();
            Debug.Log($"Hit at grid coordinates: {hitGridCoordinates}");
            if (_grid.IsEmptyAtArea(hitArea))
            {
                var newBlockPosition = Grid.GridCoordinatesToWorldPosition(hitGridCoordinates);
                var rotation = _blockPreview.transform.rotation;
                Instantiate(blockPrefab, newBlockPosition, rotation);
                _grid.PlaceAtArea(hitArea);
                SelectBlock(blockPrefab);
            }
        }
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;

        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        Debug.Log(turnAngle);

        _blockPreview.transform.Rotate(Vector3.up, turnAngle, Space.World);

        RotateBlockPositions(turnDirection, Vector3Int.up);
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;
        
        var turnAngle = Mathf.Sign(turnDirection) * 90f;

        var axis = _cam.transform.forward.z > _cam.transform.forward.x ? Vector3Int.right : Vector3Int.forward;
        _blockPreview.transform.Rotate(axis, turnAngle, Space.World);
        
        RotateBlockPositions(turnDirection, axis);
    }

    private void RotateBlockPositions(float turnDirection, Vector3Int axis)
    {
        _rotatedBlockPositions = _rotatedBlockPositions.ConvertAll(pos => pos.Rotate(axis, Mathf.RoundToInt(turnDirection)));
    }
}