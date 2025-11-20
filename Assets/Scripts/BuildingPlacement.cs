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
    [SerializeField] private CameraControls cameraControls;
    
    private Grid _grid;
    private Camera _cam;
    private GameObject _blockPreview;
    private List<Vector3Int> _rotatedBlockPositions;
    private Vector3Int? _lastHoveredGridCoordinates = null;
    private Vector3Int blockOffset = new Vector3Int(0, 0, 0);
    private int _currentFloor;

    public const float FLOOR_HEIGHT = 1f;
    
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
        playerInput.OnFloorChanged += AdjustPlacementHeightToFloorChange;
    }

    private void OnDisable()
    {
        playerInput.OnMousePositionChanged -= CheckForEmptyBuildingSlot;
        playerInput.OnMouseClicked -= TryPlaceBuilding;
        playerInput.OnFloorChanged -= AdjustPlacementHeightToFloorChange;
    }

    public void SelectBlock(BuildingBlock block)
    {
        if (_blockPreview != null)
            Destroy(_blockPreview);
        
        blockPrefab = block;
        _blockPreview = Instantiate(block.PreviewPrefab);
        _blockPreview.SetActive(false);
        _rotatedBlockPositions = new List<Vector3Int>(blockPrefab.Positions);
    }

    private void AdjustPlacementHeightToFloorChange(float floorChange)
    {
        var newFloor = (int)(_currentFloor + floorChange);
        if (newFloor < 0)
            return;
        
        _currentFloor = newFloor;
        cameraControls.SetNewHeight(_currentFloor * FLOOR_HEIGHT);
    }

    private void CheckForEmptyBuildingSlotOnFloor(int floor, Vector2 mousePosition)
    {
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        if (!mouseRay.TryGetPositionY(floor * FLOOR_HEIGHT, out var hitCoordinates))
            return;
        
        hitCoordinates += new Vector3(0, 0.1f, 0);
        var gridCoordinates = Grid.WorldPositionToGridCoordinates(hitCoordinates);
        if (_lastHoveredGridCoordinates != null && gridCoordinates == _lastHoveredGridCoordinates.Value)
            return;
        
        debugHoverText.text = $"Hovering:{gridCoordinates}";
        _lastHoveredGridCoordinates = gridCoordinates;
        var hitArea = GetHitArea(gridCoordinates);
        if (_grid.IsEmptyAtArea(hitArea))
        {
            _blockPreview.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
            _blockPreview.SetActive(true);
            return;
        }

        _lastHoveredGridCoordinates = null;
        _blockPreview.SetActive(false);
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        CheckForEmptyBuildingSlotOnFloor(_currentFloor, mousePosition);
        // var mouseRay = _cam.ScreenPointToRay(mousePosition);
        // if (Physics.Raycast(mouseRay, out var hit))
        // {
        //     var hitGridCoordinates = Grid.WorldPositionToGridCoordinates(hit.point + 0.1f * hit.normal);
        //     if (_lastHoveredGridCoordinates != null && hitGridCoordinates == _lastHoveredGridCoordinates.Value)
        //         return;
        //     
        //     debugHoverText.text = $"Hovering:{hitGridCoordinates}";
        //     _lastHoveredGridCoordinates = hitGridCoordinates;
        //     var hitArea = GetHitArea(hitGridCoordinates);
        //     if (_grid.IsEmptyAtArea(hitArea))
        //     {
        //         _blockPreview.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
        //         _blockPreview.SetActive(true);
        //         return;
        //     }
        //
        //     _lastHoveredGridCoordinates = null;
        // }
        // else
        // {
        //     _lastHoveredGridCoordinates = null;
        // }
        // _blockPreview.SetActive(false);
    }

    private List<Vector3Int> GetHitArea(Vector3Int gridCoordinates)
    {
        return _rotatedBlockPositions.Select(pos => pos + gridCoordinates - blockOffset).ToList();
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
        var newBlockPosition = Grid.GridCoordinatesToWorldPosition(coordinate) - blockOffset;
        return newBlockPosition;
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
        var camZ = _cam.transform.position.z;
        var camX = _cam.transform.position.x;

        var axis = Mathf.Abs(camZ) > Mathf.Abs(camX) ? 
            -Mathf.RoundToInt(Mathf.Sign(camZ)) * Vector3Int.right : 
            Mathf.RoundToInt(Mathf.Sign(camX)) * Vector3Int.forward;
        _blockPreview.transform.Rotate(axis, turnAngle, Space.World);
        
        //FIX ROtation Directions be same
        RotateBlockPositions(turnDirection, axis);
    }

    private void RotateBlockPositions(float turnDirection, Vector3Int axis)
    {
        _rotatedBlockPositions = _rotatedBlockPositions.ConvertAll(pos => pos.Rotate(axis, Mathf.RoundToInt(turnDirection)));
        blockOffset = _rotatedBlockPositions.Aggregate(Vector3Int.zero, (acc, v) => v.y < acc.y ? v : acc);
        if (blockOffset.y >= 0)
        {
            blockOffset = Vector3Int.zero;
        }
        
        if(_lastHoveredGridCoordinates != null)
            _blockPreview.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
    }
}