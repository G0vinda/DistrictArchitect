using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using PlasticGui;
using PlasticGui.WorkspaceWindow.Items;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    [CustomEditor(typeof(WfcConfig))]
    public class WfcConfigEditor : Editor
    {
        [field: SerializeField] public Texture2D AxisIcon32 { get; set; }
        
        [SerializeField] private VisualTreeAsset mainVisualTree;
        [SerializeField] private VisualTreeAsset subBlockListElementTemplate;

        private Button _addSubBlockButton;
        private Label _subBlockCountText;
        private ListView _subBlockListView;
        private VisualElement _root;
        private List<DataRow> _subBlocksDataCopy = new();
        private List<DataRow> _filteredSubBlockList = new();
        private VisualElement _currentAllowedNeighborsList;
        private ToggleBar _buildingFilterBar;
        private ToggleBar _subBlockTypeFilterBar;
        private int _lastExpandedListId = -1;
        private bool _needsRepaint;
        private List<BuildingType> _filteredBuildingTypes = new();
        private List<SubBlockType> _filteredSubBlockTypes = new();
        
        private void OnEnable()
        {
            _subBlocksDataCopy = ((WfcConfig)target).SubBlockTable.AsEnumerable().ToList(); 
            ((WfcConfig)target).SubBlockTableUpdated += RefreshSubBlockListView;
        }

        private void OnDisable()
        {
            ((WfcConfig)target).SubBlockTableUpdated -= RefreshSubBlockListView;
        }
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            mainVisualTree.CloneTree(_root);
            
            _addSubBlockButton = _root.Q<Button>("AddSubBlockButton");
            _addSubBlockButton.RegisterCallback<ClickEvent>(OnButtonClick);

            _subBlockCountText = _root.Q<Label>("SubBlockCountText");
            _subBlockCountText.text = $"SubBlocks added: {((WfcConfig)target).GetSubBlockCount()}";
            
            _buildingFilterBar = _root.Q<ToggleBar>("BuildingFilterBar");
            foreach (BuildingType buildingType in Enum.GetValues(typeof(BuildingType)))
            {
                var filterToggle = _buildingFilterBar.AddToggle(buildingType.ToString());
                filterToggle.ValueChangedOnClick += newValue => OnBuildingTypeFilterChanged(buildingType, newValue);
                filterToggle.DoubleClicked += () =>
                {
                    _buildingFilterBar.SetAllTogglesActive(false);
                    filterToggle.Value = true;
                    SetFilterToSingleBuildingType(buildingType);
                };
                _filteredBuildingTypes.Add(buildingType);
            }
            
            _subBlockTypeFilterBar = _root.Q<ToggleBar>("SubBlockTypeFilterBar");
            foreach (SubBlockType subBlockType in Enum.GetValues(typeof(SubBlockType)))
            {
                var filterToggle = _subBlockTypeFilterBar.AddToggle(subBlockType.ToString());
                filterToggle.ValueChangedOnClick += newValue => ChangeFilterValueForSubBlockType(subBlockType, newValue);
                filterToggle.DoubleClicked += () =>
                {
                    _subBlockTypeFilterBar.SetAllTogglesActive(false);
                    filterToggle.Value = true;
                    SetFilterToSingleSubBlockType(subBlockType);
                };
                _filteredSubBlockTypes.Add(subBlockType);
            }
            
            _subBlockListView = _root.Q<ListView>("SubBlockListView");
            SetupSubBlockList();
            
            return _root;
        }

        private void OnButtonClick(ClickEvent evt)
        {
            var popUp = CreateInstance<SubBlockAddWindow>();
            popUp.WfcConfig = (WfcConfig)target;
            popUp.Show();
        }

        private void SetupSubBlockList()
        {
            _subBlockListView.makeItem = () => subBlockListElementTemplate.CloneTree();
            _filteredSubBlockList = _subBlocksDataCopy;
            _subBlockListView.bindItem = (element, i) =>
            {
                var nameLabel = element.Q<Label>("PrefabNameLabel");
                var subBlockPrefab = (GameObject)_filteredSubBlockList.ElementAt(i)[WfcConfig.PREFAB_COLUMN_INDEX]; 
                nameLabel.text = subBlockPrefab.name;
                
                SetListElementImageToDefault(element, subBlockPrefab);
                
                SetCallbackToListElementButton(element, () => OnAllowedNeighborsClicked(element, i), "AllowedNeighborsButton");
                SetCallbackToListElementButton(element, () => OnDeleteClicked(element, i), "DeleteButton");
            };
            _subBlockListView.itemsSource = _filteredSubBlockList;
            _subBlockListView.selectionType = SelectionType.None;
        }

        private void SetCallbackToListElementButton(VisualElement listElement, Action callback, string buttonIdentifier)
        {
            var button = listElement.Q<Button>(buttonIdentifier);
            if (button.userData != null)
                button.clicked -= listElement.userData as Action;

            button.userData = callback;
            button.clicked += callback;
        }

        private void OnAllowedNeighborsClicked(VisualElement subBlockListElement, int listIndex)
        {
            var newNeighborListParent = subBlockListElement.Q<VisualElement>("AllowedNeighborListContainer");
            if (_currentAllowedNeighborsList != null)
            {
                var previousNeighborListParent = _currentAllowedNeighborsList.parent;
                var previousSubBlockListElement = previousNeighborListParent.parent;
                
                previousNeighborListParent.Remove(_currentAllowedNeighborsList);
                var lastExpandedSubBlock = _filteredSubBlockList.FirstOrDefault(subBlock => (int)subBlock[WfcConfig.ID_COLUMN_INDEX] == _lastExpandedListId);
                if (lastExpandedSubBlock != null)
                {
                    var previousPrefab = (GameObject)lastExpandedSubBlock[WfcConfig.PREFAB_COLUMN_INDEX];
                    SetListElementImageToDefault(previousSubBlockListElement, previousPrefab);   
                }
                
                if (previousNeighborListParent == newNeighborListParent)
                {
                    _currentAllowedNeighborsList = null;
                    return;
                }
            }

            var clickedSubBlock = _filteredSubBlockList.ElementAt(listIndex);
            var prefab = (GameObject)clickedSubBlock[WfcConfig.PREFAB_COLUMN_INDEX];
            SetListElementImageToMaximized(subBlockListElement, prefab);
            _lastExpandedListId = (int)clickedSubBlock[WfcConfig.ID_COLUMN_INDEX];
            _currentAllowedNeighborsList =
                new AllowedNeighborList((WfcConfig)target, (int)clickedSubBlock[WfcConfig.ID_COLUMN_INDEX]);
            newNeighborListParent.Add(_currentAllowedNeighborsList);
        }

        private void OnDeleteClicked(VisualElement elementToDelete, int listIndex)
        {
            var subBlockRow = _filteredSubBlockList.ElementAt(listIndex);
            var subBlockId = (int)subBlockRow[WfcConfig.ID_COLUMN_INDEX];
            var prefab = (GameObject)subBlockRow[WfcConfig.PREFAB_COLUMN_INDEX];
            
            var confirmWindow = CreateInstance<SubBlockDeleteConfirmWindow>();
            confirmWindow.SetSubBlockName(prefab.name);
            confirmWindow.ConfirmedButtonPressed += () => ((WfcConfig)target).RemoveSubBlock(subBlockId);
            var confirmWindowWidth = 270;
            var confirmWindowHeight = 150;
            var mainWindow = EditorGUIUtility.GetMainWindowPosition();
            var xPos = mainWindow.x + (mainWindow.width - confirmWindowWidth) * 0.5f;
            var yPos = mainWindow.y + (mainWindow.height - confirmWindowHeight) * 0.5f;
            confirmWindow.position = new Rect(xPos, yPos, confirmWindowWidth, confirmWindowHeight);
            confirmWindow.ShowPopup();
        }

        private void OnBuildingTypeFilterChanged(BuildingType buildingType, bool newValue)
        {
            if (newValue)
                _filteredBuildingTypes.Add(buildingType);
            else
                _filteredBuildingTypes.Remove(buildingType);
            
            RefreshSubBlockListView();
        }
        
        private void SetFilterToSingleBuildingType(BuildingType buildingType)
        {
            _filteredBuildingTypes = new List<BuildingType>() { buildingType };
            RefreshSubBlockListView();
        }

        private void ChangeFilterValueForSubBlockType(SubBlockType subBlockType, bool newValue)
        {
            if (newValue)
                _filteredSubBlockTypes.Add(subBlockType);
            else
                _filteredSubBlockTypes.Remove(subBlockType);
            
            RefreshSubBlockListView();
        }
        
        private void SetFilterToSingleSubBlockType(SubBlockType subBlockType)
        {
            _filteredSubBlockTypes = new List<SubBlockType>() { subBlockType };
            RefreshSubBlockListView();
        }

        private void SetListElementImageToDefault(VisualElement element, GameObject prefab)
        {
            var prefabPreviewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 50);
            SetListElementImage(element, prefabPreviewTexture);
        }

        private void SetListElementImageToMaximized(VisualElement element, GameObject prefab)
        {
            var prefabPreviewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 100);
            prefabPreviewTexture.ApplyOtherTextureInBottomRightCorner(AxisIcon32);
            SetListElementImage(element, prefabPreviewTexture);
        }

        private static void SetListElementImage(VisualElement listElement, Texture2D texture)
        {
            var previewImageContainer = listElement.Q<VisualElement>("PreviewImageContainer");

            if (previewImageContainer.childCount > 0)
            {
                previewImageContainer.Remove(previewImageContainer.Children().First());
            }
            
            var previewImage = new Image
            {
                style =
                {
                    width = texture.width,
                    height = texture.height,
                },
                image = texture,
                scaleMode = ScaleMode.StretchToFill,
                tintColor = Color.white
            };
            previewImageContainer.Add(previewImage);
        }

        private void RefreshSubBlockListView()
        {
            _subBlockListView.itemsSource = null;
            _subBlocksDataCopy = ((WfcConfig)target).SubBlockTable.AsEnumerable().ToList();
            _subBlockCountText.text = $"SubBlocks added: {_subBlocksDataCopy.Count}";
            _filteredSubBlockList.Clear();
            
            foreach (var dataRow in _subBlocksDataCopy)
            {
                if (!_filteredBuildingTypes.Contains((BuildingType)dataRow[WfcConfig.BUILDING_COLUMN_INDEX]))
                    continue;
                
                if (!_filteredSubBlockTypes.Contains((SubBlockType)dataRow[WfcConfig.TYPE_COLUMN_INDEX]))
                    continue;
                
                _filteredSubBlockList.Add(dataRow);
            }
            _subBlockListView.itemsSource = _filteredSubBlockList;
            _subBlockListView.Rebuild();
        }
    }
}
