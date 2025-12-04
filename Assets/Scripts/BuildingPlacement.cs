using System;
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
    [SerializeField] GameObject buildingBlockPrefab;
    [SerializeField] GameObject buildingBlockPreviewPrefab;
    [SerializeField] TextMeshProUGUI debugHoverText;
    [SerializeField] private CameraControls cameraControls;
    [SerializeField] private Material buildingPreviewMaterial;
    [SerializeField] private Material buildingPreviewDisabledMaterial;
    
    public const float FLOOR_HEIGHT = 1f;

    public Action PlacedBuilding;
    
    private Grid _grid;
    private Camera _cam;
    private GameObject _blockPreview;
    private List<Vector3Int> _rotatedBlockPositions;
    private int _currentFloor;
    private Vector3Int? _lastHoveredGridCoordinates;
    private bool _placeable;
    private int _blockYAdjustment;
    private Quaternion _desiredPreviewRotation;
    private Coroutine _previewRotationCoroutine;
    
    private List<Vector3Int> _currentBlockPositions;
    private Material _currentBlockMaterial;
    
    private void Start()
    {
        _grid = new Grid();
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

    public void SelectBlockShape(List<Vector3Int> blockPositions, Material blockMaterial)
    {
        _blockYAdjustment = 0;
        _desiredPreviewRotation = Quaternion.identity;
        _rotatedBlockPositions = new List<Vector3Int>(blockPositions);
        
        if (_blockPreview != null)
            Destroy(_blockPreview);
        
        _blockPreview = Instantiate(buildingBlockPreviewPrefab);
        _blockPreview.gameObject.SetActive(false);
        _blockPreview.GetComponent<MeshFilter>().mesh = BlockMeshGenerator.Generate(blockPositions);
        _currentBlockPositions = blockPositions;
        _currentBlockMaterial = blockMaterial;
    }

    public void Unselect()
    {
        Destroy(_blockPreview);
        _currentBlockPositions = null;
    }

    private void AdjustPlacementHeightToFloorChange(float floorChange)
    {
        var newFloor = (int)(_currentFloor + floorChange);
        if (newFloor < 0 || newFloor >= Grid.MAP_SIZE)
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

        var floorY = floor * FLOOR_HEIGHT;
        var mouseRay = _cam.ScreenPointToRay(mousePosition);
        // if (Physics.Raycast(mouseRay, out RaycastHit hit) && 
        //     hit.point.y > floorY + float.Epsilon && 
        //     hit.point.y < floorY + FLOOR_HEIGHT)
        // {
        //     
        // }
        // else
        // {
        //     
        // }
        
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
            if (_previewRotationCoroutine != null)
                StopCoroutine(_previewRotationCoroutine);
            _blockPreview.SetActive(false);
            _blockPreview.transform.rotation = _desiredPreviewRotation;
            return;
        }
        
        debugHoverText.text = $"Hovering:{gridCoordinates}";
        _lastHoveredGridCoordinates = gridCoordinates;
        UpdatePreview();
    }

    private void UpdatePreview()
    {
        if (_lastHoveredGridCoordinates == null || _blockPreview == null)
            return;
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value); 
        _blockPreview.transform.position = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment);
        _blockPreview.SetActive(true);

        _placeable = _grid.IsEmptyAtArea(hitArea);
        _blockPreview.GetComponent<MeshRenderer>().material = _placeable ? buildingPreviewMaterial : buildingPreviewDisabledMaterial;
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
        if (!_placeable || _blockPreview == null) return;
        
        if (_previewRotationCoroutine != null)
            StopCoroutine(_previewRotationCoroutine);
        
        var hitArea = GetHitArea(_lastHoveredGridCoordinates.Value).ToList();
        var newBlockPosition = GetBlockPositionFromCoordinate(_lastHoveredGridCoordinates.Value + Vector3Int.up * _blockYAdjustment);
        var newBuildingBlock = Instantiate(buildingBlockPrefab, newBlockPosition, _blockPreview.transform.rotation);
        var mesh = BlockMeshGenerator.Generate(_currentBlockPositions);
        newBuildingBlock.GetComponent<MeshFilter>().mesh = mesh;
        newBuildingBlock.GetComponent<MeshCollider>().sharedMesh = mesh;
        newBuildingBlock.GetComponent<MeshRenderer>().material = _currentBlockMaterial;
        _grid.PlaceAtArea(hitArea);
        Unselect();
        
        PlayPlacementAnimation(newBuildingBlock);
        PlacedBuilding?.Invoke();
    }

    private void PlayPlacementAnimation(GameObject placedBlock)
    {
        var placementAnimation = DOTween.Sequence();
        var whiteBlendId = Shader.PropertyToID("_WhiteBlend");
        var material = placedBlock.GetComponent<MeshRenderer>().material;
        material.SetFloat(whiteBlendId, 0.6f);
        placementAnimation.Append(placedBlock.transform.DOScale(Vector3.one * 1.15f, 0.05f));
        placementAnimation.Append(placedBlock.transform.DOScale(Vector3.one * 1f, 0.15f).SetEase(Ease.OutSine));
        placementAnimation.Join(DOVirtual.Float(0.6f, 0f, 0.15f, value => material.SetFloat(whiteBlendId, value))
            .SetEase(Ease.OutSine));
    }

    private Vector3 GetBlockPositionFromCoordinate(Vector3Int coordinate)
    {
        var newBlockPosition = Grid.GridCoordinatesToWorldPosition(coordinate);
        return newBlockPosition;
    }

    private void TurnPreviewHorizontally(float turnDirection)
    {
        if (!_blockPreview) return;

        var turnAngle = Mathf.Sign(turnDirection) * 90f;
        RotatePreviewModel(turnAngle, Vector3Int.up);

        RotateBlockPositions(turnDirection, Vector3Int.up);
        UpdatePreview();
    }

    private void TurnPreviewVertically(float turnDirection)
    {
        if (!_blockPreview) return;
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

    private void OnDrawGizmos()
    {
        if (_lastHoveredGridCoordinates == null || _blockPreview == null)
            return;
        
        var positions = GetHitArea(_lastHoveredGridCoordinates.Value);
        foreach (var pos in positions)
        {
            Gizmos.DrawCube(GetBlockPositionFromCoordinate(pos), Vector3.one);
        }
    }

    private void RotateBlockPositions(float turnDirection, Vector3Int axis)
    {
        _rotatedBlockPositions = _rotatedBlockPositions.ConvertAll(pos => pos.Rotate(axis, Mathf.RoundToInt(turnDirection)));
        _blockYAdjustment = _rotatedBlockPositions.Min(pos => pos.y) * -1;
    }
}