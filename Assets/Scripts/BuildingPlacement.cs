using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] CameraControls camControls;
    [SerializeField] PlayerInput playerInput;
    [SerializeField] BuildingBlock blockPrefab;
    [SerializeField] TextMeshProUGUI debugHoverText;
    [SerializeField] private CameraControls cameraControls;
    [SerializeField] private Color errorColor = Color.red;
    
    public const float FLOOR_HEIGHT = 1f;
    
    private Grid _grid;
    private Camera _cam;
    private GameObject _blockPreview;
    private List<Vector3Int> _rotatedBlockPositions;
    private int _currentFloor;
    private Vector3Int? _lastHoveredGridCoordinates;
    private bool _placeable;
    private Color _baseColor;
    private int _blockYAdjustment;
    private Quaternion _desiredPreviewRotation;
    private Coroutine _previewRotationCoroutine;
    
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
        _baseColor = _blockPreview.gameObject.GetComponentsInChildren<MeshRenderer>()[0].material.color;
        _blockPreview.SetActive(false);
        _blockYAdjustment = 0;
        _desiredPreviewRotation = Quaternion.identity;
        _rotatedBlockPositions = new List<Vector3Int>(blockPrefab.Positions);
    }

    private void AdjustPlacementHeightToFloorChange(float floorChange)
    {
        var newFloor = (int)(_currentFloor + floorChange);
        if (newFloor < 0)
            return;
        
        _currentFloor = newFloor;
        cameraControls.SetNewHeight(_currentFloor * FLOOR_HEIGHT);
        _lastHoveredGridCoordinates += Vector3Int.up * (int)floorChange;
        UpdatePreview();
    }

    private void CheckForEmptyBuildingSlotOnFloor(int floor, Vector2 mousePosition)
    {
        if(_blockPreview == null)
            return;
        
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        if (!mouseRay.TryGetPositionAtY(floor * FLOOR_HEIGHT, out var hitCoordinates))
            return;
        
        hitCoordinates += new Vector3(0, 0.1f, 0);
        var gridCoordinates = Grid.WorldPositionToGridCoordinates(hitCoordinates);
        if (_lastHoveredGridCoordinates != null && gridCoordinates == _lastHoveredGridCoordinates.Value)
            return;

        if (!Grid.IsCoordinateInGrid(gridCoordinates))
        {
            _lastHoveredGridCoordinates = null;
            _placeable = false;
            _blockPreview.SetActive(false);
            _desiredPreviewRotation = Quaternion.identity;
            return;
        }
        
        debugHoverText.text = $"Hovering:{gridCoordinates}";
        _lastHoveredGridCoordinates = gridCoordinates;
        UpdatePreview();
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

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        CheckForEmptyBuildingSlotOnFloor(_currentFloor, mousePosition);
    }

    private List<Vector3Int> GetHitArea(Vector3Int gridCoordinates)
    {
        return _rotatedBlockPositions.Select(pos => pos + gridCoordinates + _blockYAdjustment * Vector3Int.up).ToList();
    }

    private void TryPlaceBuilding()
    {
        if (!_placeable) return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value).ToList();
        var newBlockPosition = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value);
        var newBuildingBlock = Instantiate(blockPrefab, newBlockPosition, _blockPreview.transform.rotation);
        _grid.PlaceAtArea(hitArea);
        SelectBlock(blockPrefab);
        
        var placementAnimation = DOTween.Sequence();
        placementAnimation.Append(newBuildingBlock.transform.DOScale(Vector3.one * 1.15f, 0.05f));
        placementAnimation.Append(newBuildingBlock.transform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutSine));
    }

    private Vector3 GetBlockPositionFromCoordinate(Vector3Int coordinate)
    {
        var newBlockPosition = Grid.GridCoordinatesToWorldPosition(coordinate) + _blockYAdjustment * Vector3.up;
        return newBlockPosition;
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;

        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        RotatePreviewModel(turnAngle, Vector3Int.up);

        RotateBlockPositions(turnDirection, Vector3Int.up);
        UpdatePreview();
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!_blockPreview.activeSelf) return;
        turnDirection *= -1;
        
        var turnAngle = Mathf.Sign(turnDirection) * 90f;

        var cameraAngle = camControls.CurrentRotationEulers.y;
        var axis = Vector3Int.left.Rotate(Vector3Int.up, 
            Mathf.RoundToInt(cameraAngle / 90));
        
        RotatePreviewModel(turnAngle, axis);
        
        //FIX ROtation Directions be same
        RotateBlockPositions(turnDirection, axis);
        UpdatePreview();
    }

    private void RotatePreviewModel(float angle, Vector3Int axis)
    {
        if (_previewRotationCoroutine != null)
            StopCoroutine(_previewRotationCoroutine);
        
        _blockPreview.transform.rotation = _desiredPreviewRotation;
        
        var newRot = Quaternion.AngleAxis(angle, axis);
        _desiredPreviewRotation = newRot * _desiredPreviewRotation;
        _previewRotationCoroutine = StartCoroutine(PreviewRotationRoutine());
    }

    private IEnumerator PreviewRotationRoutine()
    {
        var t = 0f;
        var rotationSpeed = 3f;
        do
        {
            t += rotationSpeed * Time.deltaTime;
            _blockPreview.transform.rotation = Quaternion.Slerp(_blockPreview.transform.rotation, _desiredPreviewRotation, t);
            yield return null;
        } while (t <= 1f);
    }

    private void RotateBlockPositions(float turnDirection, Vector3Int axis)
    {
        _rotatedBlockPositions = _rotatedBlockPositions.ConvertAll(pos => pos.Rotate(axis, Mathf.RoundToInt(turnDirection)));
        _blockYAdjustment = _rotatedBlockPositions.Min(pos => pos.y) * -1;
    }
}