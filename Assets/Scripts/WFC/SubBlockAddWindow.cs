using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    public class SubBlockAddWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree;
        [SerializeField] private Texture2D axesIconTexture;
        
        public WfcConfig WfcConfig { get; set; }

        private Button _addSubBlockButton;
        private ObjectField _prefabField;
        private EnumField _typeField;
        private FloatField _probabilityField;
        private Label _couldNotReadTypeLabel;
        private VisualElement _imageContainer;
        private Image _currentPreviewImage;

        private const int AXES_ICON_PADDING = 5;
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
            _typeField = rootVisualElement.Q<EnumField>("TypeField");
            _probabilityField = rootVisualElement.Q<FloatField>("ProbabilityField");
            
            _couldNotReadTypeLabel = rootVisualElement.Q<Label>("CouldNotReadTypeLabel");
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
                var prefabPreviewTexture = GetPrefabPreviewTexture((GameObject)evt.newValue);
                ApplyAxesIconOnTexture(prefabPreviewTexture);
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

                if (SubBlockUtils.TryExtractSubBlockTypeFromName(((GameObject)_prefabField.value).name,
                        out var subBlockType))
                {
                    _typeField.value = subBlockType;
                }
                else
                {
                    _couldNotReadTypeLabel.style.display = DisplayStyle.Flex;
                }
                _imageContainer.Add(_currentPreviewImage);
                _addSubBlockButton.SetEnabled(true);
            }
            else
            {
                _couldNotReadTypeLabel.style.display = DisplayStyle.None;
            }
            _addSubBlockButton.text = ADD_TEXT;
        }

        private static Texture2D GetPrefabPreviewTexture(GameObject prefab)
        {
            var editor = Editor.CreateEditor(prefab);
            var prefabPath = AssetDatabase.GetAssetPath(prefab);
            var texture = editor.RenderStaticPreview(prefabPath, null, 200, 200);
            DestroyImmediate(editor);
            
            return texture;
        }

        private void ApplyAxesIconOnTexture(Texture2D texture)
        {
            var xStart = texture.width - axesIconTexture.width - AXES_ICON_PADDING;
            var yStart = AXES_ICON_PADDING;
            
            var axesIconPixels = axesIconTexture.GetPixels();
            var pixelIndex = 0;
            for (var y = 0; y < axesIconTexture.height; y++)
            {
                for (var x = 0; x < axesIconTexture.width; x++)
                {
                    var axesIconPixel = axesIconPixels[pixelIndex]; 
                    if (axesIconPixel.a > float.Epsilon)
                    {
                        texture.SetPixel(x + xStart, y + yStart, axesIconPixel);   
                    }
                    
                    pixelIndex++; 
                }
            }
            texture.Apply();
        }

        private void AddButtonPressed(ClickEvent evt)
        {
            WfcConfig.AddSubBlock((SubBlockType)_typeField.value, (GameObject)_prefabField.value, _probabilityField.value);
            _addSubBlockButton.text = ADDED_TEXT;
            _addSubBlockButton.SetEnabled(false);
        }
    }
}