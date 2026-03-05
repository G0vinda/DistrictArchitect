using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WFC.Editor;

namespace WFC
{
    public class SubBlockAddWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private Texture2D axesIcon64;
        
        public WfcConfig WfcConfig { get; set; }

        private Button _addSubBlockButton;
        private ObjectField _prefabField;
        private EnumField _buildingField;
        private EnumField _typeField;
        private FloatField _probabilityField;
        private Label _couldNotReadTypeLabel;
        private Label _couldNotReadBuildingTypeLabel;
        private Label _prefabAlreadyExistsLabel;
        private VisualElement _imageContainer;
        private Image _currentPreviewImage;

        private const string ADD_TEXT = "Add";
        private const string ADDED_TEXT = "Added";
        
        private void CreateGUI()
        {
            visualTree.CloneTree(rootVisualElement);
            
            _addSubBlockButton = rootVisualElement.Q<Button>("AddButton");
            _addSubBlockButton.RegisterCallback<ClickEvent>(AddButtonPressed);

            _addSubBlockButton.SetEnabled(false);
            
            _prefabField = rootVisualElement.Q<ObjectField>("PrefabField");
            _prefabField.RegisterCallback<ChangeEvent<Object>>(OnPrefabChanged);
            _buildingField = rootVisualElement.Q<EnumField>("BuildingField");
            _typeField = rootVisualElement.Q<EnumField>("TypeField");
            _probabilityField = rootVisualElement.Q<FloatField>("ProbabilityField");
            
            _couldNotReadTypeLabel = rootVisualElement.Q<Label>("CouldNotReadTypeLabel");
            _couldNotReadBuildingTypeLabel = rootVisualElement.Q<Label>("CouldNotReadBuildingTypeLabel");
            _prefabAlreadyExistsLabel = rootVisualElement.Q<Label>("PrefabAlreadyExistsLabel");
            _imageContainer = rootVisualElement.Q<VisualElement>("ImageContainer");
        }

        private void OnPrefabChanged(ChangeEvent<Object> evt)
        {
            if (_currentPreviewImage != null)
            {
                _imageContainer.Remove(_currentPreviewImage);
                _currentPreviewImage = null;
            }
            if (evt.newValue != null)
            {
                var newPrefab = (GameObject)evt.newValue;
                var prefabPreviewTexture = EditorUtils.GetPrefabPreviewTexture(newPrefab, 200);
                prefabPreviewTexture.ApplyOtherTextureInBottomRightCorner(axesIcon64);
                _currentPreviewImage = new Image
                {
                    style =
                    {
                        width = prefabPreviewTexture.width,
                        height = prefabPreviewTexture.height,
                    },
                    image = prefabPreviewTexture,
                    scaleMode = ScaleMode.StretchToFill,
                    tintColor = Color.white
                };
                
                if (WfcConfig.DoesPrefabExistInDatabase(newPrefab))
                {
                    _prefabAlreadyExistsLabel.style.display = DisplayStyle.Flex;
                    _prefabField.AddToClassList("warningInputField");
                }
                else
                {
                    _prefabAlreadyExistsLabel.style.display = DisplayStyle.None;
                    _prefabField.RemoveFromClassList("warningInputField");
                }

                if (SubBlockUtils.TryExtractSubBlockTypeFromName(newPrefab.name, out var subBlockType))
                {
                    _typeField.value = subBlockType;
                    _couldNotReadTypeLabel.style.display = DisplayStyle.None;
                }
                else
                {
                    _couldNotReadTypeLabel.style.display = DisplayStyle.Flex;
                }

                if (SubBlockUtils.TryExtractBuildingTypeFromName(newPrefab.name, out var buildingType))
                {
                    _buildingField.value = buildingType;
                    _couldNotReadBuildingTypeLabel.style.display = DisplayStyle.None;
                }
                else
                {
                    _couldNotReadBuildingTypeLabel.style.display = DisplayStyle.Flex;
                }
                _imageContainer.Add(_currentPreviewImage);
                _addSubBlockButton.SetEnabled(true);
            }
            else
            {
                _couldNotReadTypeLabel.style.display = DisplayStyle.None;
                _couldNotReadBuildingTypeLabel.style.display = DisplayStyle.None;
                _prefabAlreadyExistsLabel.style.display = DisplayStyle.None;
                _prefabField.RemoveFromClassList("warningInputField");
            }
            _addSubBlockButton.text = ADD_TEXT;
        }

        private void AddButtonPressed(ClickEvent evt)
        {
            WfcConfig.AddSubBlock(
                (BuildingType)_buildingField.value,
                (SubBlockType)_typeField.value,
                (GameObject)_prefabField.value,
                _probabilityField.value);
            
            _addSubBlockButton.text = ADDED_TEXT;
            _addSubBlockButton.SetEnabled(false);
        }
    }
}