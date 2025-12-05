using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ClusterEvaluationEffect : MonoBehaviour
{
    [SerializeField] private GameObject selectionUI;
    [SerializeField] private BuildingPlacement buildingPlacement;
    [SerializeField] private CellClusterSelector cellClusterSelector;
    [SerializeField] private float cameraDistanceToBlock;
    [SerializeField] private float cameraSpeed;
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI clusterScoreText;

    private float minCameraShakeStrength = 0.05f;
    private float maxCameraShakeStrength = 1f;
    private float minCameraShakeDuration = 0.05f;
    private float maxCameraShakeDuration = 0.15f;
    
    private float minScoreSpeed = 0.2f;
    private float maxScoreSpeed = 0.01f;
    
    private int maxEffektCount = 15;


    private Vector3 _desiredCameraPosition;
    private Camera _camera;

    private int totalScore = 0;
    private int currentClusterScore = 0;

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
        validationSequence.AppendCallback(() => totalScoreText.text = "Total Score: 0");
        
        var cellCoordinates = grid.GetAllCellCoordinates();
        while (cellCoordinates.Count > 0)
        {
            var maxY = cellCoordinates.Max(coord => coord.y);
            var current = cellCoordinates.First(coord => coord.y == maxY);
            var currentCluster = grid.FindCellCluster(current);
            
            ClusterAnimation(validationSequence, currentCluster, cellCoordinates);
        }

        validationSequence.AppendCallback(() =>
        {
            clusterScoreText.text = totalScoreText.text;
            totalScoreText.text = "";
        });

        StartCoroutine(CameraFollowAnimation());
    }
    
    private void ClusterAnimation(Sequence validationSequence, List<Vector3Int> currentCluster, List<Vector3Int> cellCoordinates)
    {
        var grid = buildingPlacement.Grid;
        var scorePerCube = 0;
        var groupByYZ = new Dictionary<Vector2Int, int>();
        var groupByXZ = new Dictionary<Vector2Int, int>();
        var groupByXY = new Dictionary<Vector2Int, int>();

        validationSequence.AppendCallback(() =>
        {
            currentClusterScore = 0;
            clusterScoreText.text = currentClusterScore.ToString();
        });
        
        validationSequence.AppendCallback(() => SetNewCameraAim(grid.GetCellObjectAtCoordinates(currentCluster[0]).transform.position));
        validationSequence.AppendCallback(() => HighlightNewCells(currentCluster));
        if (currentCluster.Count > 1)
        {
            validationSequence.AppendInterval(0.4f);
            for (var i = 0; i < currentCluster.Count; i++)
            {
                var point = currentCluster[i];
                var camEffektIntensity = Mathf.InverseLerp(0, maxEffektCount - 1, i);

                var speed = Mathf.Lerp(minScoreSpeed, maxScoreSpeed, camEffektIntensity);
                AddScoreAnimationForPosition(validationSequence, point, scorePerCube, speed);

                scorePerCube++;

                TryScoreRow(validationSequence, groupByYZ, new Vector2Int(point.y, point.z), 0);
                TryScoreRow(validationSequence, groupByXZ, new Vector2Int(point.x, point.z), 1);
                TryScoreRow(validationSequence, groupByXY, new Vector2Int(point.x, point.y), 2);
            }
        }

        validationSequence.AppendInterval(0.5f);
        
        for (var i = 0; i < currentCluster.Count; i++)
        {
            var cell = grid.GetCellObjectAtCoordinates(currentCluster[i]);

            var camEffektIntensity = Mathf.InverseLerp(0, maxEffektCount - 1, i);

            AddDestroyBlockSequence(validationSequence, cell, camEffektIntensity);
            
            cellCoordinates.Remove(currentCluster[i]);
        }
    }

    private void AddDestroyBlockSequence(Sequence validationSequence, CellObject cell, float camEffektIntensity)
    {            
        validationSequence.AppendCallback(() => SetNewCameraAim(cell.transform.position));
        var speed = Mathf.Lerp(minScoreSpeed, maxScoreSpeed, camEffektIntensity);
        validationSequence.AppendInterval(speed);
        validationSequence.AppendCallback(() => TransferValueToTotalScore(cell));
        validationSequence.AppendCallback(cell.DestroyWithVfx); 
        var camShakeDuration = Mathf.Lerp(minCameraShakeDuration, maxCameraShakeDuration, camEffektIntensity);
        var camShakeStrength = Mathf.Lerp(minCameraShakeStrength, maxCameraShakeStrength, camEffektIntensity);
        validationSequence.Append(_camera.DOShakePosition(camShakeDuration, Vector3.one * camShakeStrength));
    }

    private void TransferValueToTotalScore(CellObject cell)
    {
        var value = cell.GetValue();
        currentClusterScore -= value;
        totalScore += value;

        clusterScoreText.text = currentClusterScore.ToString();
        totalScoreText.text = "Total Score: " + totalScore;
    }

    private void AddScoreAnimationForPosition(Sequence validationSequence, Vector3Int position, int scoreToAdd, float speed)
    {
        var grid = buildingPlacement.Grid;
        
        validationSequence.AppendCallback(() =>
            SetNewCameraAim(grid.GetCellObjectAtCoordinates(position).transform.position));
        validationSequence.AppendInterval(speed);
        
        var cell = grid.GetCellObjectAtCoordinates(position);
        validationSequence.Append(cell.transform.DOScale(Vector3.one * 1.15f, speed /4));
        validationSequence.Append(cell.transform.DOScale(Vector3.one * 1f, speed * 3/4).SetEase(Ease.OutSine));
        
        validationSequence.JoinCallback(() =>
        {
            currentClusterScore += scoreToAdd;
            clusterScoreText.text = currentClusterScore.ToString();
        });
        validationSequence.JoinCallback(() => cell.IncreaseValue(scoreToAdd));
    }

    private void TryScoreRow(Sequence sequence, Dictionary<Vector2Int, int> dictionary, Vector2Int key, int dim)
    {
        if (!dictionary.TryAdd(key, 1))
            dictionary[key]++;

        if (dictionary[key] < Grid.MAP_SIZE) 
            return;

        for (var i = 0; i < Grid.MAP_SIZE; i++)
        {
            var position = dim switch
            {
                0 => new Vector3Int(i, key.x, key.y),
                1 => new Vector3Int(key.x, i, key.y),
                2 => new Vector3Int(key.x, key.y, i),
                _ => Vector3Int.zero
            };

            AddScoreAnimationForPosition(sequence, position, 1, 0.05f);
        }
    }

    private void HighlightNewCells(List<Vector3Int> highLightCoordinates) // Diese Methode ist etwas duplicate code. Es gibt die gleiche nochmal im CellClusterSelector
    {
        var ignoreTransparency = 0.15f;
        foreach (var cellObject1 in buildingPlacement.Grid.GetAllCellObjects())
        {
            cellObject1.SetAlpha(ignoreTransparency);
            cellObject1.SetCastShadows(false);
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