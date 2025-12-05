using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] private CameraControls camControls;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject buildingBlockPrefab;
    [SerializeField] private GameObject buildingBlockPreviewPrefab;
    [SerializeField] private TextMeshProUGUI debugHoverText;
    [SerializeField] private CameraControls cameraControls;
    [SerializeField] private Material buildingPreviewMaterial;
    [SerializeField] private Material buildingPreviewDisabledMaterial;
    [SerializeField] private float previewMaterialAlpha = 0.6f;
    [SerializeField] private UnityEvent<int> floorChanged; 
    
    public bool IsPlacing => _currentShapeObject != null;
    public Grid Grid { get; private set; }
    
    public Action<Vector3> PlacedBuilding;
    public Action GameOver;
    
    private Camera _cam;
    private int _currentFloor;
    private Vector3Int? _lastHoveredGridCoordinates;
    private bool _placeable;
    private int _blockYAdjustment;
    private ShapeObject _currentShapeObject;
    
    private const float FLOOR_HEIGHT = 1f;
    
    private void Start()
    {
        Grid = new Grid();
        _cam = Camera.main;
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

    public void SelectBlockShape(Dictionary<Vector3Int, CellData> cellDataByPositions)
    {
        _blockYAdjustment = 0;
        
        if (_currentShapeObject)
            Destroy(_currentShapeObject);
        
        _currentShapeObject = ShapeObjectGenerator.Instance.Generate(cellDataByPositions);
        _currentShapeObject.SetMaterialAlpha(previewMaterialAlpha);
        _currentShapeObject.Hide();
    }

    public void Unselect()
    {
        if (_currentShapeObject)
            Destroy(_currentShapeObject.gameObject);
    }

    private void AdjustPlacementHeightToFloorChange(float floorChange)
    {
        var newFloor = (int)(_currentFloor + floorChange);
        if (newFloor < 0 || newFloor >= Grid.MAP_SIZE)
            return;
        
        _currentFloor = newFloor;
        cameraControls.SetNewHeight(_currentFloor * FLOOR_HEIGHT);
        floorChanged?.Invoke(_currentFloor);
        _lastHoveredGridCoordinates += Vector3Int.up * (int)floorChange;
        UpdatePreview();
    }

    private void CheckForEmptyBuildingSlotOnFloor(int floor, Vector2 mousePosition)
    {
        if(!_currentShapeObject)
            return;

        var floorY = floor * FLOOR_HEIGHT;
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        
        if (!mouseRay.TryGetPositionAtY(floorY, out var hitCoordinates))
            return;
        
        hitCoordinates += new Vector3(0, 0.1f, 0);
        var gridCoordinates = Grid.WorldPositionToGridCoordinates(hitCoordinates);
        if (_lastHoveredGridCoordinates != null && gridCoordinates == _lastHoveredGridCoordinates.Value)
            return;

        if (!Grid.IsCoordinateInGrid(gridCoordinates))
        {
            _lastHoveredGridCoordinates = null;
            _placeable = false;
            _currentShapeObject.Hide();
            return;
        }
        
        debugHoverText.text = $"Hovering:{gridCoordinates}";
        _lastHoveredGridCoordinates = gridCoordinates;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (!_currentShapeObject || !_lastHoveredGridCoordinates.HasValue)
            return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value); 
        
        _currentShapeObject.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment);
        _currentShapeObject.Show();

        _placeable = Grid.CanShapeBePlacedAtArea(hitArea);
        _currentShapeObject.SetMaterialToDisabled(!_placeable);
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        CheckForEmptyBuildingSlotOnFloor(_currentFloor, mousePosition);
    }

    private List<Vector3Int> GetHitArea(Vector3Int gridCoordinates)
    {
        return _currentShapeObject.CellCoordinates.Select(coord => coord + gridCoordinates + _blockYAdjustment * Vector3Int.up).ToList();
    }

    private void TryPlaceBuilding()
    {
        if (!_placeable || !_currentShapeObject || !_lastHoveredGridCoordinates.HasValue) 
            return;

        var finalShapeCoordinates = _lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment;
        var finalShapePosition = GetBlockPositionFromCoordinate(finalShapeCoordinates);
        _currentShapeObject.transform.position = finalShapePosition;
        var isGameOver = Grid.PlaceShapeAtPosition(_currentShapeObject, finalShapeCoordinates);
        _currentShapeObject.SetMaterialAlpha(1.0f);
        _currentShapeObject.EnableColliders();
        PlayPlacementAnimation();
        _currentShapeObject = null;
        
        PlacedBuilding?.Invoke(finalShapePosition);
        if(isGameOver) GameOver?.Invoke();
    }

    private void PlayPlacementAnimation()
    {
        var placementAnimation = DOTween.Sequence();
        _currentShapeObject.SetMaterialWhiteBlend(0.6f);
        var animationShapeReference = _currentShapeObject;
        placementAnimation.Append(animationShapeReference.transform.DOScale(Vector3.one * 1.15f, 0.05f));
        placementAnimation.Append(animationShapeReference.transform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutSine));
        placementAnimation.Join(DOVirtual.Float(0.6f, 0f, 0.15f, value => animationShapeReference.SetMaterialWhiteBlend(value))
            .SetEase(Ease.OutSine));
    }

    private Vector3 GetBlockPositionFromCoordinate(Vector3Int coordinate)
    {
        var newBlockPosition = Grid.GridCoordinatesToWorldPosition(coordinate);
        return newBlockPosition;
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        Debug.Log("Should be Turning Preview Horizontally");
        if (!_currentShapeObject)
            return;
        
        _currentShapeObject.Rotate90(Vector3Int.up, Mathf.RoundToInt(turnDirection));
        UpdatePreview();
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!_currentShapeObject)
            return;
        
        turnDirection *= -1;
        
        var cameraAngle = camControls.CurrentRotationEulers.y;
        var axis = Vector3Int.left.Rotate90(Vector3Int.up, 
            Mathf.RoundToInt(cameraAngle / 90));
        _currentShapeObject.Rotate90(axis, Mathf.RoundToInt(turnDirection));
        var minY = _currentShapeObject.CellCoordinates.Min(coord => coord.y);
        _blockYAdjustment = -minY;
        UpdatePreview();
    }
}