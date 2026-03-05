using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC.Editor
{
    [CustomEditor(typeof(WfcConfig))]
    public class WfcConfigEditor : UnityEditor.Editor
    {
        [field: SerializeField] public Texture2D AxesIcon32 { get; set; }
        
        [SerializeField] private VisualTreeAsset mainVisualTree;
        [SerializeField] private VisualTreeAsset subBlockListElementTemplate;
        [SerializeField] private VisualTreeAsset variantListElementTemplate;

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
            _addSubBlockButton.RegisterCallback<ClickEvent>(OnSubBlockAddButtonClicked);

            _subBlockCountText = _root.Q<Label>("SubBlockCountText");
            _subBlockCountText.text = $"SubBlocks added: {((WfcConfig)target).SubBlockCount}";
            
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

        private void OnSubBlockAddButtonClicked(ClickEvent evt)
        {
            var window = CreateInstance<SubBlockAddWindow>();
            window.WfcConfig = (WfcConfig)target;
            window.Show();
        }

        private void SetupSubBlockList()
        {
            _subBlockListView.makeItem = () => subBlockListElementTemplate.CloneTree();
            _filteredSubBlockList = _subBlocksDataCopy;
            _subBlockListView.bindItem = (element, i) =>
            {
                var nameLabel = element.Q<Label>("PrefabNameLabel");
                var subBlockPrefabs = (GameObject[])_filteredSubBlockList.ElementAt(i)[WfcConfig.PREFAB_COLUMN_INDEX];
                if (subBlockPrefabs[0])
                {
                    nameLabel.text = subBlockPrefabs[0].name;
                    SetListElementToDefault(element, subBlockPrefabs[0]);
                }
                else
                {
                    nameLabel.text = "Prefab is missing!";
                }
                
                var variantCountLabel = element.Q<Label>("VariantCountLabel");
                variantCountLabel.text = $"{subBlockPrefabs.Length - 1} Variant{(subBlockPrefabs.Length != 2 ? "s" : "")}";
                
                SetCallbackToListElementButton(element, () => OnSubBlockEditClicked(element, i), "EditButton");
                SetCallbackToListElementButton(element, () => OnAddVariantToSubBlockClicked(i), "AddVariantButton");
                SetCallbackToListElementButton(element, () => OnDeleteSubBlockClicked(element, i), "DeleteButton");

                var subBlockType = (SubBlockType)_filteredSubBlockList.ElementAt(i)[WfcConfig.TYPE_COLUMN_INDEX];
                if (subBlockType is SubBlockType.BottomCorner or SubBlockType.TopCorner or SubBlockType.MiddleEdge)
                {
                    var mirrorButton = element.Q<Button>("MirrorButton");
                    mirrorButton.style.display = DisplayStyle.Flex;
                    SetCallbackToListElementButton(element, () => OnMirrorSubBlockClicked(i), "MirrorButton");   
                }
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
        
        private void OnAddVariantToSubBlockClicked(int index)
        {
            var addSubBlockVariantWindow = CreateInstance<SubBlockVariantAddWindow>();
            addSubBlockVariantWindow.WfcConfig = (WfcConfig)target;
            addSubBlockVariantWindow.SubBlockId = (int)_filteredSubBlockList.ElementAt(index)[WfcConfig.ID_COLUMN_INDEX];
            addSubBlockVariantWindow.Show();
        }

        private void OnMirrorSubBlockClicked(int index)
        {
            // Todo: implement
        }

        private void OnSubBlockEditClicked(VisualElement subBlockListElement, int listIndex)
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
                    var previousPrefab = ((GameObject[])lastExpandedSubBlock[WfcConfig.PREFAB_COLUMN_INDEX])[0];
                    SetListElementToDefault(previousSubBlockListElement, previousPrefab);   
                }
                
                if (previousNeighborListParent == newNeighborListParent)
                {
                    _currentAllowedNeighborsList = null;
                    return;
                }
            }

            var clickedSubBlock = _filteredSubBlockList.ElementAt(listIndex);
            SetListElementToMaximized(subBlockListElement, clickedSubBlock);
            _lastExpandedListId = (int)clickedSubBlock[WfcConfig.ID_COLUMN_INDEX];
            _currentAllowedNeighborsList =
                new AllowedNeighborList((WfcConfig)target, (int)clickedSubBlock[WfcConfig.ID_COLUMN_INDEX]);
            newNeighborListParent.Add(_currentAllowedNeighborsList);
        }

        private void OnDeleteSubBlockClicked(VisualElement elementToDelete, int listIndex)
        {
            var subBlockRow = _filteredSubBlockList.ElementAt(listIndex);
            var subBlockId = (int)subBlockRow[WfcConfig.ID_COLUMN_INDEX];
            var prefab = ((GameObject[])subBlockRow[WfcConfig.PREFAB_COLUMN_INDEX])[0];
            
            var confirmWindow = CreateInstance<SubBlockDeleteConfirmWindow>();
            confirmWindow.SetSubBlockName(prefab != null ? prefab.name : "Missing Prefab");
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

        private void SetListElementToDefault(VisualElement element, GameObject prefab)
        {
            var prefabPreviewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 50);
            SetListElementImage(element, prefabPreviewTexture);
            
            element.Q<VisualElement>("TextContainer").style.display = DisplayStyle.Flex;
            // Todo : update variant text
            element.Q<VisualElement>("VariantContainer").style.display = DisplayStyle.None;
        }

        private void SetListElementToMaximized(VisualElement element, DataRow dataRow)
        {
            var prefabs = (GameObject[])dataRow[WfcConfig.PREFAB_COLUMN_INDEX];
            var probabilities = (float[])dataRow[WfcConfig.PROBABILITIES_COLUMN_INDEX];
            var prefabPreviewTexture = EditorUtils.GetPrefabPreviewTexture(prefabs[0], 100);
            prefabPreviewTexture.ApplyOtherTextureInBottomRightCorner(AxesIcon32);
            SetListElementImage(element, prefabPreviewTexture);
            
            element.Q<VisualElement>("TextContainer").style.display = DisplayStyle.None;
            var variantContainer = element.Q<VisualElement>("VariantContainer");
            variantContainer.Clear();
            variantContainer.style.display = DisplayStyle.Flex;
            
            for (var i = 0; i < prefabs.Length; i++)
            {
                variantListElementTemplate.CloneTree(variantContainer);
                var newVariantListElement = variantContainer.Query<VisualElement>("VariantListElement").Build().ElementAt(i);
                var probabilityField = newVariantListElement.Q<FloatField>("ProbabilityField");
                probabilityField.value = probabilities[i];
                var localScopeI = i;
                probabilityField.RegisterCallback<ChangeEvent<float>>(evt => ((WfcConfig)target).ChangeSubBlockVariantProbability((int)dataRow[WfcConfig.ID_COLUMN_INDEX], localScopeI, evt.newValue));
                
                var nameLabel = newVariantListElement.Q<Label>("NameLabel");
                nameLabel.text = prefabs[i].name;
                if (i == 0)
                {
                    newVariantListElement.EnableInClassList("variant-list-element", false);
                    newVariantListElement.EnableInClassList("variant-list-element-main", true);
                }
                else
                {
                    var previewImageContainer = newVariantListElement.Q<VisualElement>("PreviewImageContainer");
                    var previewTexture = EditorUtils.GetPrefabPreviewTexture(prefabs[i], 40);
                    previewImageContainer.Add(EditorUtils.GenerateImage(previewTexture));
                    
                    var deleteButton = newVariantListElement.Q<Button>("DeleteButton");
                    deleteButton.clicked += () =>
                        ((WfcConfig)target).RemoveSubBlockVariant((int)dataRow[WfcConfig.ID_COLUMN_INDEX], localScopeI);
                }
            }
        }

        private static void SetListElementImage(VisualElement listElement, Texture2D texture)
        {
            var previewImageContainer = listElement.Q<VisualElement>("PreviewImageContainer");

            if (previewImageContainer.childCount > 0)
            {
                previewImageContainer.Remove(previewImageContainer.Children().First());
            }
            
            previewImageContainer.Add(EditorUtils.GenerateImage(texture));
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
