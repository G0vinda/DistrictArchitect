using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace WFC.Editor
{
    public class AllowedNeighborToolWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset mainVisualTreeAsset;
        [SerializeField] private GameObject subBlockPlaceholderPrefab;
        [SerializeField] private GameObject highLightPrefab;
        [SerializeField] private SubBlockPreviewNameInfo previewNameInfo;

        private int _mainId;
        private WfcConfig _wfcConfig;
        private Label _headerLabel;
        private VisualElement _previewImageContainer;
        private VisualElement _defaultButtonContainer;
        private VisualElement _continueButtonContainer;
        private Button _continueButton;
        private Scene _previewScene;
        private Camera _previewCamera;
        private IEnumerator<Image> _previewImageEnumerator;
        private SubBlockPreviewNameInfo _previewNameInfo;
        private readonly Dictionary<Vector3Int, GameObject> _placeHolderBlocksByCoordinate = new();
        private DataRow _mainDataRow;
        private Vector3Int _currentNeighborDirection;
        private int _currentNeighborId;
        private Stack<DataRow> _subBlockRowsToProcess;
        
        private void CreateGUI()
        {
            mainVisualTreeAsset.CloneTree(rootVisualElement);
            _headerLabel = rootVisualElement.Q<Label>("HeaderLabel");
            _previewImageContainer = rootVisualElement.Q<VisualElement>("PreviewImageContainer");
            _defaultButtonContainer = rootVisualElement.Q<VisualElement>("ButtonContainer_Default");
            _continueButtonContainer = rootVisualElement.Q<VisualElement>("ButtonContainer_Continue");
            
            var allowButton = rootVisualElement.Q<Button>("AllowButton");
            allowButton.clicked += OnAllowClicked;
            var forbidButton = rootVisualElement.Q<Button>("ForbidButton");
            forbidButton.clicked += OnForbidClicked;
            _continueButton = rootVisualElement.Q<Button>("ContinueButton");
            _continueButton.clicked += OnContinueClicked;
            
            _previewScene = EditorSceneManager.NewPreviewScene();
            _previewCamera = new GameObject("PreviewCamera").AddComponent<Camera>();
            _previewCamera.cameraType = CameraType.Game;
            _previewCamera.scene = _previewScene;
            _previewCamera.clearFlags = CameraClearFlags.SolidColor;
            _previewCamera.backgroundColor = Color.grey;
            SceneManager.MoveGameObjectToScene(_previewCamera.gameObject, _previewScene);
            _previewCamera.targetTexture = new RenderTexture(600, 600, 24);
            _previewNameInfo = PrefabUtility.InstantiatePrefab(previewNameInfo, _previewScene) as SubBlockPreviewNameInfo;
            _previewNameInfo.SetCamera(_previewCamera);
            
            for (var x = -1; x < 2; x++)
            {
                for (var y = -1; y < 2; y++)
                {
                    for (var z = -1; z < 2; z++)
                    {
                        var coordinate = new Vector3Int(x, y, z);
                        var placeholder = PrefabUtility.InstantiatePrefab(subBlockPlaceholderPrefab, _previewScene) as GameObject;
                        placeholder.transform.position = (Vector3)coordinate * _wfcConfig.subBlockSize;
                        placeholder.transform.localScale = Vector3.one * _wfcConfig.subBlockSize;
                        _placeHolderBlocksByCoordinate.Add(coordinate, placeholder);
                    }
                }   
            }
            
            SetupForNextSubBlock();
        }

        private void OnAllowClicked()
        {
            if (!Array.Exists((int[])_mainDataRow[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[_currentNeighborDirection]], id => id == _currentNeighborId))
                _wfcConfig.AddAllowedNeighbor(_currentNeighborId, _mainId, _currentNeighborDirection);
            
            PreviewNextNeighbor();
        }

        private void OnForbidClicked()
        {
            if (Array.Exists((int[])_mainDataRow[WfcConfig.NEIGHBOR_COLUMN_INDEX_BY_DIRECTION[_currentNeighborDirection]], id => id == _currentNeighborId))
                _wfcConfig.RemoveAllowedNeighbor(_currentNeighborId, _mainId, _currentNeighborDirection);
            
            PreviewNextNeighbor();
        }

        private void OnContinueClicked()
        {
            SetupForNextSubBlock();
            _continueButtonContainer.style.display = DisplayStyle.None;
            _defaultButtonContainer.style.display = DisplayStyle.Flex;
        }

        private void SetSubBlockRowsToProcess(List<DataRow> subBlockRows)
        {
            _subBlockRowsToProcess = new Stack<DataRow>();
            for (var i = subBlockRows.Count - 1; i >= 0; i--)
            {
                _subBlockRowsToProcess.Push(subBlockRows[i]);
            }
        }

        private void SetupForNextSubBlock()
        {
            _mainDataRow = _subBlockRowsToProcess.Pop();
            _mainId = (int)_mainDataRow[WfcConfig.ID_COLUMN_INDEX];
            var prefab = ((GameObject[])_mainDataRow[WfcConfig.PREFAB_COLUMN_INDEX])[0];
            _headerLabel.text = $"Selecting allowed neighbors for <b>{prefab.name}</b>";
            
            var previewImages = ImagesPreviewingNeighborsFor(_mainId);
            _previewImageEnumerator = previewImages.GetEnumerator();
            PreviewNextNeighbor();
        }

        private IEnumerable<Image> ImagesPreviewingNeighborsFor(int id)
        {
            var subBlockType = (SubBlockType)_mainDataRow[WfcConfig.TYPE_COLUMN_INDEX];
            var prefab = ((GameObject[])_mainDataRow[WfcConfig.PREFAB_COLUMN_INDEX])[0];
            var currentSubBlock = PrefabUtility.InstantiatePrefab(prefab, _previewScene) as GameObject;
            var currentBlockCoordinates = subBlockType.GetDefaultCoordinate();
            currentSubBlock.transform.position = (Vector3)currentBlockCoordinates * _wfcConfig.subBlockSize;
            _placeHolderBlocksByCoordinate[currentBlockCoordinates].SetActive(false);
            
            var subBlocks = _wfcConfig.SubBlockTable.AsEnumerable();

            

            foreach (var direction in Vector3IntUtils.Directions)
            {
                _currentNeighborDirection = direction;
                var neighborTypeInDirection = subBlockType.GetNeighborTypeInDirection(direction);
                var possibleNeighborsInDirection = subBlocks.Where(row => (SubBlockType)row[WfcConfig.TYPE_COLUMN_INDEX] == neighborTypeInDirection).ToList();

                var betweenPoint = currentSubBlock.transform.position + (Vector3)direction * _wfcConfig.subBlockSize * 0.5f;
                Vector3 cameraOffset;
                if (direction.x != 0)
                    cameraOffset = new Vector3(-direction.x, 1, 4);
                else if (direction.z != 0)
                    cameraOffset = new Vector3(4, 1, -direction.z);
                else
                    cameraOffset = new Vector3(3, 1 + direction.y, 3);
                
                _previewCamera.transform.position = betweenPoint + cameraOffset * _wfcConfig.subBlockSize;
                _previewCamera.transform.LookAt(betweenPoint);
                
                var neighborCoordinates = currentBlockCoordinates + direction;
                var neighborPosition = currentSubBlock.transform.position + (Vector3)direction * _wfcConfig.subBlockSize;
                var neighbor90Rotations = neighborTypeInDirection.GetDefaultCoordinate().Get90RotationsAroundYTo(neighborCoordinates.GetWrappedNeg1To1());
                var neighborRotation = Quaternion.Euler(0, neighbor90Rotations * 90, 0);
                
                if (_placeHolderBlocksByCoordinate.TryGetValue(neighborCoordinates, out var placeHolder))
                    placeHolder.SetActive(false);
                
                var neighborHighlight = PrefabUtility.InstantiatePrefab(highLightPrefab, _previewScene) as GameObject;
                neighborHighlight.transform.position = neighborPosition;
                neighborHighlight.transform.localScale = Vector3.one * _wfcConfig.subBlockSize;
                _previewNameInfo.SetName("Empty Space");
                
                _currentNeighborId = WfcConfig.EMPTY_SUB_BLOCK_ID;
                _previewCamera.Render();
                var previewImage = EditorUtils.GenerateImage(_previewCamera.targetTexture);
                yield return previewImage;
                
                foreach (var possibleNeighborRow in possibleNeighborsInDirection)
                {
                    _currentNeighborId = (int)possibleNeighborRow[WfcConfig.ID_COLUMN_INDEX];
                    var firstNeighborPrefab = ((GameObject[])possibleNeighborRow[WfcConfig.PREFAB_COLUMN_INDEX])[0];
                
                    var currentNeighborSubBlock = PrefabUtility.InstantiatePrefab(firstNeighborPrefab, _previewScene) as GameObject;
                    currentNeighborSubBlock.transform.position = neighborPosition;   
                    currentNeighborSubBlock.transform.rotation = neighborRotation;
                    
                    _previewNameInfo.SetName(currentNeighborSubBlock.name);
                    
                    _previewCamera.Render();
                    previewImage = EditorUtils.GenerateImage(_previewCamera.targetTexture);
                    yield return previewImage;
                    
                    DestroyImmediate(currentNeighborSubBlock);
                }
                
                if (placeHolder)
                    placeHolder.SetActive(true);
                
                DestroyImmediate(neighborHighlight);
            }
            DestroyImmediate(currentSubBlock);
            _placeHolderBlocksByCoordinate[currentBlockCoordinates].SetActive(true);
        }

        private void PreviewNextNeighbor()
        {
            _previewImageContainer.Clear();
            if (_previewImageEnumerator.MoveNext())
            {
                _previewImageContainer.Add(_previewImageEnumerator.Current);                
            }
            else
            {
                var finishedLabel = new Label();
                finishedLabel.text = "All possible neighbors processed.";
                _previewImageContainer.Add(finishedLabel);
                _defaultButtonContainer.style.display = DisplayStyle.None;
                _continueButtonContainer.style.display = DisplayStyle.Flex;
                
                _continueButton.SetEnabled(_subBlockRowsToProcess.Count > 0);
            }
        }

        private void OnDisable()
        {
            EditorSceneManager.CloseScene(_previewScene, true);
            Debug.Log("Editor disabled");
        }

        public static void Create(List<DataRow> selectedRows, WfcConfig wfcConfig)
        {
            var window = CreateInstance<AllowedNeighborToolWindow>();
            window.titleContent = new GUIContent("Allowed Neighbor Editor");
            window._wfcConfig = wfcConfig;
            window.SetSubBlockRowsToProcess(selectedRows);

            var size = new Vector2(600, 670);
            window.minSize = size;
            window.maxSize = size;
            
            window.ShowModalUtility();
        }
    }
}