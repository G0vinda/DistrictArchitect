using UnityEngine;

public class BlockSelection : MonoBehaviour
{
    [SerializeField] private BlockSelectionField[] selectionFields;
    [SerializeField] private ShapeManager shapeManager;
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private CellClusterSelector cellClusterSelector;
    [SerializeField] private PlayerInput playerInput;
    
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
            blockSelectionField.SetShapeDefinition(shapeManager.GetRandomShapeDefinition());
        }
    }
    
    private void CancelSelection()
    {
        if (!_selectedField)
            return;
        
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
        buildingPlacement.SelectBlockShape(selectionField.ShapeDefinition);
        cellClusterSelector.ResetHighlighting();
        _selectedField = selectionField;
    }
    
    private void OnBuildingPlaced(Vector3 _)
    {
        _selectedField.SetShapeDefinition(shapeManager.GetRandomShapeDefinition());
        CancelSelection();
    }
}