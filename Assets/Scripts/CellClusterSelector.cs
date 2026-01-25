using UnityEngine;

public class CellClusterSelector : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private float ignoreTransparency = 0.15f;

    private bool _highlighted;
        
    private void OnEnable()
    {
        playerInput.OnMouseClicked += OnMouseClick;
        playerInput.OnCancelled += ResetHighlighting;
    }
    
    private void OnDisable()
    {
        playerInput.OnMouseClicked -= OnMouseClick;
        playerInput.OnCancelled -= ResetHighlighting;
    }

    private void OnMouseClick()
    {
        if (buildingPlacement.IsPlacing)
            return;
        
        var mouseRay = Camera.main!.ScreenPointToRay(playerInput.MousePosition);
        if (!Physics.Raycast(mouseRay, out RaycastHit hit) || !hit.collider.gameObject.GetComponent<Cell>())
        {
            ResetHighlighting();
            return;
        }

        
        var gridCoordinates = Grid.WorldPositionToGridCoordinates(hit.collider.transform.position);
        Debug.Log($"Hit grid at: {gridCoordinates}");
        HighlightCluster(gridCoordinates);
    }

    private void HighlightCluster(Vector3Int startCoordinates)
    {
        var clusterCoordinates = buildingPlacement.Grid.FindCellCluster(startCoordinates);
        foreach (var cellObject in buildingPlacement.Grid.GetAllCellObjects())
        {
            cellObject.SetAlpha(ignoreTransparency);
            cellObject.SetCastShadows(false);
        }
        
        foreach (var clusterCoordinate in clusterCoordinates)
        {
            var cellObject = buildingPlacement.Grid.GetCellObjectAtCoordinates(clusterCoordinate);
            cellObject.SetAlpha(1.0f);
            cellObject.SetCastShadows(true);
        }
        _highlighted = true;
    }

    public void ResetHighlighting()
    {
        if (!_highlighted)
            return;
        
        foreach (var cellObject in buildingPlacement.Grid.GetAllCellObjects())
        {
            cellObject.SetAlpha(1.0f);
            cellObject.SetCastShadows(true);
        }
        _highlighted = false;
    }
}