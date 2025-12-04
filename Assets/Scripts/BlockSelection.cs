using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class BlockSelection : MonoBehaviour
{
    [SerializeField] private BlockSelectionField[] selectionFields;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Material[] blockMaterials;
    
    private BlockSelectionField _selectedField;

    private void OnEnable()
    {
        playerInput.OnRightClicked += CancelSelection;
        buildingPlacement.PlacedBuilding += OnBuildingPlaced;
    }

    private void OnDisable()
    {
        playerInput.OnRightClicked -= CancelSelection;
        buildingPlacement.PlacedBuilding -= OnBuildingPlaced;
    }

    private void Start()
    {
        foreach (var blockSelectionField in selectionFields)
        {
            blockSelectionField.SetBlockShape(shapeManager.GetRandomShape(), blockMaterials[Random.Range(0, blockMaterials.Length)]);
        }
    }
    
    private void CancelSelection()
    {
        _selectedField.SetHighlight(false);
        buildingPlacement.Unselect();
        _selectedField = null;
    }

    public void OnSelectionFieldClicked(BlockSelectionField selectionField)
    {
        if (selectionField == _selectedField)
        {
            CancelSelection();
            return; 
        }
        
        if (_selectedField != null)
            _selectedField.SetHighlight(false);
        selectionField.SetHighlight(true);
        buildingPlacement.SelectBlockShape(selectionField.BlockPositions, selectionField.BlockMaterial);
        _selectedField = selectionField;
    }
    
    private void OnBuildingPlaced()
    {
        _selectedField.SetBlockShape(shapeManager.GetRandomShape(), blockMaterials[Random.Range(0, blockMaterials.Length)]);
        CancelSelection();
    }
}