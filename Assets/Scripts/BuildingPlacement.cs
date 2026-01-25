using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] private CameraControls camControls;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private TextMeshProUGUI debugHoverText;
    [SerializeField] private CameraControls cameraControls;
    [SerializeField] private Material buildingPreviewMaterial;
    [SerializeField] private Material buildingPreviewDisabledMaterial;
    [SerializeField] private float previewMaterialAlpha = 0.6f;
    [SerializeField] private UnityEvent<int> floorChanged; 
    
    public bool IsPlacing => currentShape != null;
    public Grid Grid { get; private set; }
    
    public Action<Vector3> PlacedBuilding;
    public Action GameOver;

    private Camera _cam;
    private int _currentFloor;
    private Vector3Int? _lastHoveredGridCoordinates;
    private bool _placeable;
    private int _blockYAdjustment;
    private Shape currentShape;
    private bool _freezePreviewUpdate;
    
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
        playerInput.OnCameraRotationStarted += FreezePreviewUpdate;
        cameraControls.OnCameraSnappedInPlace += UnfreezePreviewUpdate;
    }

    private void OnDisable()
    {
        playerInput.OnMousePositionChanged -= CheckForEmptyBuildingSlot;
        playerInput.OnMouseClicked -= TryPlaceBuilding;
        playerInput.OnVerticalTurn -= TurnPreviewVertically;
        playerInput.OnHorizontalTurn -= TurnPreviewHorizontally;
        playerInput.OnFloorChanged -= AdjustPlacementHeightToFloorChange;
        playerInput.OnCameraRotationStarted -= FreezePreviewUpdate;
        cameraControls.OnCameraSnappedInPlace -= UnfreezePreviewUpdate;
    }

    public void SelectBlockShape(Dictionary<Vector3Int, Building> cellDataByPositions, int nRightRotations)
    {
        _blockYAdjustment = 0;
        
        if (currentShape)
            Destroy(currentShape);
        
        currentShape = ShapeGenerator.Instance.Generate(cellDataByPositions);
        currentShape.SetMaterialAlpha(previewMaterialAlpha);
        for (int i = 0; i < nRightRotations; i++)
        {
            currentShape.Rotate90(Vector3Int.up, 1);
        }
        
        currentShape.Hide();
    }

    public void Unselect()
    {
        if (currentShape)
            Destroy(currentShape.gameObject);
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
        if(!currentShape)
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
            currentShape.Hide();
            return;
        }
        
        debugHoverText.text = $"Hovering:{gridCoordinates}";
        _lastHoveredGridCoordinates = gridCoordinates;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (!currentShape || !_lastHoveredGridCoordinates.HasValue)
            return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value); 
        
        currentShape.transform.position = Grid.GridCoordinatesToWorldPosition(_lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment);
        currentShape.Show();

        _placeable = Grid.CanShapeBePlacedAtArea(hitArea);
        currentShape.SetMaterialToDisabled(!_placeable);
    }

    private void CheckForEmptyBuildingSlot(Vector2 mousePosition)
    {
        if (_freezePreviewUpdate)
            return;
        CheckForEmptyBuildingSlotOnFloor(_currentFloor, mousePosition);
    }

    private List<Vector3Int> GetHitArea(Vector3Int gridCoordinates)
    {
        return currentShape.CellCoordinates.Select(coord => coord + gridCoordinates + _blockYAdjustment * Vector3Int.up).ToList();
    }

    private void TryPlaceBuilding()
    {
        if (!_placeable || !currentShape || !_lastHoveredGridCoordinates.HasValue) 
            return;

        var finalShapeCoordinates = _lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment;
        var finalShapePosition = Grid.GridCoordinatesToWorldPosition(finalShapeCoordinates);
        currentShape.transform.position = finalShapePosition;
        var isGameOver = Grid.PlaceShapeAtPosition(currentShape, finalShapeCoordinates);
        currentShape.SetMaterialAlpha(1.0f);
        currentShape.EnableColliders();
        PlayPlacementAnimation();
        currentShape = null;
        
        PlacedBuilding?.Invoke(finalShapePosition);
        if(isGameOver) GameOver?.Invoke();
    }

    private void PlayPlacementAnimation()
    {
        var placementAnimation = DOTween.Sequence();
        currentShape.SetMaterialWhiteBlend(0.6f);
        var animationShapeReference = currentShape;
        placementAnimation.Append(animationShapeReference.transform.DOScale(Vector3.one * 1.15f, 0.05f));
        placementAnimation.Append(animationShapeReference.transform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutSine));
        placementAnimation.Join(DOVirtual.Float(0.6f, 0f, 0.15f, value => animationShapeReference.SetMaterialWhiteBlend(value))
            .SetEase(Ease.OutSine));
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        Debug.Log("Should be Turning Preview Horizontally");
        if (!currentShape)
            return;
        
        currentShape.Rotate90(Vector3Int.up, Mathf.RoundToInt(turnDirection));
        UpdatePreview();
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!currentShape)
            return;
        
        turnDirection *= -1;
        
        var cameraAngle = camControls.CurrentRotationEulers.y;
        var axis = Vector3Int.left.Rotate90(Vector3Int.up, 
            Mathf.RoundToInt(cameraAngle / 90));
        currentShape.Rotate90(axis, Mathf.RoundToInt(turnDirection));
        var minY = currentShape.CellCoordinates.Min(coord => coord.y);
        _blockYAdjustment = -minY;
        UpdatePreview();
    }
    
    private void FreezePreviewUpdate() => _freezePreviewUpdate = true;

    private void UnfreezePreviewUpdate()
    {
        if (_lastHoveredGridCoordinates != null && currentShape)
        {
            var lastHoveredWorldPosition = Grid.GridCoordinatesToWorldPosition(_lastHoveredGridCoordinates.Value);
            lastHoveredWorldPosition -= new Vector3(0, Grid.CELL_SIZE/2, 0);
            var newMousePosition = _cam.WorldToScreenPoint(lastHoveredWorldPosition);
            Mouse.current.WarpCursorPosition(newMousePosition);
        }

        _freezePreviewUpdate = false;
    }
}