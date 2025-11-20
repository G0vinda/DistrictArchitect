using System.Collections.Generic;
using System.Linq;
using Sirenix.Serialization;
using TMPro;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] PlayerInput playerInput;
    [SerializeField] BuildingBlock blockPrefab;
    [SerializeField] TextMeshProUGUI debugHoverText;
    [SerializeField] private Color errorColor = Color.red;

private Grid _grid;
    private Camera _cam;
    private GameObject _blockPreview;
    private List<Vector3Int> _rotatedBlockPositions;
    private Vector3Int? _lastHoveredGridCoordinates;
    private bool _placeable;
    private Color _baseColor;
    private Vector3Int _blockOffset = new(0, 0, 0);
    
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
        
        blockPrefab = block;
        _blockPreview = Instantiate(block.PreviewPrefab);
        _baseColor = _blockPreview.gameObject.GetComponentsInChildren<MeshRenderer>()[0].material.color;

        _blockPreview.SetActive(false);
        _rotatedBlockPositions = new List<Vector3Int>(blockPrefab.Positions);
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        if(_blockPreview == null)
            return;
        
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(mouseRay, out var hit))
        {
            var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point + 0.1f * hit.normal);
            if (_lastHoveredGridCoordinates != null && hitGridCoordinates == _lastHoveredGridCoordinates.Value)
                return;
            
            debugHoverText.text = $"Hovering:{hitGridCoordinates}";
            _lastHoveredGridCoordinates = hitGridCoordinates;
            UpdatePreview();
        }
        else
        {
            _lastHoveredGridCoordinates = null;
            _placeable = false;
            _blockPreview.SetActive(false);
        }
    }

    private void UpdatePreview()
    {
        if (_lastHoveredGridCoordinates == null)
            return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value); 
        _blockPreview.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
        _blockPreview.SetActive(true);

        _placeable = _grid.IsEmptyAtArea(hitArea);
        
        foreach (var meshRenderer in _blockPreview.gameObject.GetComponentsInChildren<MeshRenderer>())
        {
            meshRenderer.material.color = _placeable ? _baseColor : errorColor;
        }
    }

    private List<Vector3Int> GetHitArea(Vector3Int gridCoordinates)
    {
        return _rotatedBlockPositions.Select(pos => pos + gridCoordinates - _blockOffset).ToList();
    }

    private void TryPlaceBuilding()
    {
        if (_lastHoveredGridCoordinates == null) return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value).ToList();
        var newBlockPosition = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
        Instantiate(blockPrefab, newBlockPosition, _blockPreview.transform.rotation);
        _grid.PlaceAtArea(hitArea);
        SelectBlock(blockPrefab);
    }

    private Vector3 GetBlockPositionFromCoordinate(Vector3Int coordinate)
    {
        var newBlockPosition = Grid.GridCoordinatesToWorldPosition(coordinate) - _blockOffset;
        return newBlockPosition;
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;

        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        Debug.Log(turnAngle);

        _blockPreview.transform.Rotate(Vector3.up, turnAngle, Space.World);

        RotateBlockPositions(turnDirection, Vector3Int.up);
        UpdatePreview();
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;
        
        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        var camZ = _cam.transform.position.z;
        var camX = _cam.transform.position.x;

        var axis = Mathf.Abs(camZ) > Mathf.Abs(camX) ? 
            -Mathf.RoundToInt(Mathf.Sign(camZ)) * Vector3Int.right : 
            Mathf.RoundToInt(Mathf.Sign(camX)) * Vector3Int.forward;
        _blockPreview.transform.Rotate(axis, turnAngle, Space.World);
        
        //FIX ROtation Directions be same
        RotateBlockPositions(turnDirection, axis);
        UpdatePreview();
    }

    private void RotateBlockPositions(float turnDirection, Vector3Int axis)
    {
        _rotatedBlockPositions = _rotatedBlockPositions.ConvertAll(pos => pos.Rotate(axis, Mathf.RoundToInt(turnDirection)));
        _blockOffset = _rotatedBlockPositions.Aggregate(Vector3Int.zero, (acc, v) => v.y < acc.y ? v : acc);
        if (_blockOffset.y >= 0)
            _blockOffset = Vector3Int.zero;
    }
}