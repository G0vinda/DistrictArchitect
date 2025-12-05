using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class ClusterEvaluationEffect : MonoBehaviour
{
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private CellClusterSelector cellClusterSelector;
    [SerializeField] private float cameraDistanceToBlock;
    [SerializeField] private float cameraSpeed;

    private float minCameraShakeStrength = 0.05f;
    private float maxCameraShakeStrength = 0.35f;
    private float minCameraShakeDuration = 0.05f;
    private float maxCameraShakeDuration = 0.15f;
    private int maxEffektCount = 10;

    private Vector3 _desiredCameraPosition;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    [ContextMenu("Play End Validation")]
    public void PlayEndValidation()
    {
        buildingPlacement.Unselect();
        cellClusterSelector.ResetHighlighting();
        buildingPlacement.enabled = false;
        cellClusterSelector.enabled = false;
        selectionUI.SetActive(false);
        
        var grid = buildingPlacement.Grid;
        _desiredCameraPosition = _camera.transform.position;

        var validationSequence = DOTween.Sequence(); // Das hier ist die Animation für die End validierung. 
        
        var cellCoordinates = grid.GetAllCellCoordinates();
        while (cellCoordinates.Count > 0)
        {
            var maxY = cellCoordinates.Max(coord => coord.y);
            var current = cellCoordinates.First(coord => coord.y == maxY);
            var currentCluster = grid.FindCellCluster(current);
            validationSequence.AppendCallback(() => HighlightCells(currentCluster));
            validationSequence.AppendCallback(() => SetNewCameraAim(grid.GetCellObjectAtCoordinates(currentCluster[0]).transform.position));
            validationSequence.AppendInterval(0.8f);
            for (int i = 0; i < currentCluster.Count; i++)
            {
                var cell = grid.GetCellObjectAtCoordinates(currentCluster[i]);
                validationSequence.AppendCallback(() => cell.DestroyWithVfx()); // Hier wir eine Celle "abgerechnet", das i zeigt die Position im Cluster
                // Wenn du Text Animation einbauen willst, dann wäre hier ein guter Platz dafür
                //validationSequence.AppendCallback(() => deine punkte text funktion oder so);
                var camEffektIntensity = Mathf.InverseLerp(0, maxEffektCount - 1, i);
                var camShakeDuration = Mathf.Lerp(minCameraShakeDuration, maxCameraShakeDuration, camEffektIntensity);
                var camShakeStrength = Mathf.Lerp(minCameraShakeStrength, maxCameraShakeStrength, camEffektIntensity);
                validationSequence.Append(_camera.DOShakePosition(camShakeDuration, Vector3.one * camShakeStrength));
                validationSequence.AppendInterval(0.3f - Mathf.Min(i, 9) * 0.03f);
                var nextIndex = i + 1;
                if (currentCluster.Count > nextIndex)
                    validationSequence.AppendCallback(() => SetNewCameraAim(grid.GetCellObjectAtCoordinates(currentCluster[nextIndex]).transform.position));
                validationSequence.AppendInterval(0.3f);
                cellCoordinates.Remove(currentCluster[i]);
            }
        }

        StartCoroutine(CameraFollowAnimation());
    }

    private void HighlightCells(List<Vector3Int> highLightCoordinates) // Diese Methode ist etwas duplicate code. Es gibt die gleiche nochmal im CellClusterSelector
    {
        var ignoreTransparency = 0.15f;
        foreach (var cellObject in buildingPlacement.Grid.GetAllCellObjects())
        {
            cellObject.SetAlpha(ignoreTransparency);
            cellObject.SetCastShadows(false);
        }
    
        foreach (var highLightCoordinate in highLightCoordinates)
        {
            var cellObject = buildingPlacement.Grid.GetCellObjectAtCoordinates(highLightCoordinate);
            cellObject.SetAlpha(1.0f);
            cellObject.SetCastShadows(true);
        }
    }

    private void SetNewCameraAim(Vector3 aim)
    {
        _desiredCameraPosition = aim - _camera.transform.forward * cameraDistanceToBlock;
    }

    private IEnumerator CameraFollowAnimation()
    {
        while (true)
        {
            _camera.transform.position = Vector3.Lerp(_camera.transform.position, _desiredCameraPosition, cameraSpeed * Time.deltaTime);
            yield return null;
        }
    }
}