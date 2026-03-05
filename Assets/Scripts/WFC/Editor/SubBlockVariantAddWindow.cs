using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC.Editor
{
    public  class SubBlockVariantAddWindow : EditorWindow
    {
        public int SubBlockId { get; set; }
        public WfcConfig WfcConfig { get; set; }
        
        [SerializeField] private Texture2D axesIcon64;
        
        private Label _textLabel;
        private ObjectField _variantPrefabField;
        private FloatField _probabilityField;
        private VisualElement _previewImageContainer;
        private Button _addButton;
        
        public void CreateGUI()
        {
            _textLabel = new Label();
            _textLabel.text = "Add Sub Block Variant";
            rootVisualElement.Add(_textLabel);
            
            _variantPrefabField = new ObjectField();
            _variantPrefabField.label = "Variant Prefab";
            _variantPrefabField.objectType = typeof(GameObject);
            _variantPrefabField.RegisterCallback<ChangeEvent<Object>>(OnPrefabChanged);
            rootVisualElement.Add(_variantPrefabField);
            
            _probabilityField = new FloatField();
            _probabilityField.label = "Probability";
            _probabilityField.value = 1.0f;
            rootVisualElement.Add(_probabilityField);

            _previewImageContainer = new VisualElement();
            _previewImageContainer.style.flexGrow = 1;
            _previewImageContainer.style.alignItems = Align.Center;
            _previewImageContainer.style.marginTop = 10;
            rootVisualElement.Add(_previewImageContainer);
            
            _addButton = new Button();
            _addButton.text = "Add";
            _addButton.RegisterCallback<ClickEvent>(OnAddVariantClicked);
            _addButton.SetEnabled(false);
            _addButton.style.height = 25;
            _addButton.style.marginBottom = 10;
            _addButton.style.marginTop = 10;
            _addButton.style.marginRight = 10;
            _addButton.style.marginLeft = 10;
            rootVisualElement.Add(_addButton);
        }

        private void OnPrefabChanged(ChangeEvent<Object> evt)
        {
            var prefab = evt.newValue as GameObject;
            _addButton.SetEnabled(prefab != null);
            _addButton.text = "Add";
            
            _previewImageContainer.Clear();
            if (prefab)
            {
                var previewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 200);
                previewTexture.ApplyOtherTextureInBottomRightCorner(axesIcon64);
                _previewImageContainer.Add(EditorUtils.GenerateImage(previewTexture));
            }
        }
        
        private void OnAddVariantClicked(ClickEvent evt)
        {
            WfcConfig.AddSubBlockVariant(SubBlockId, (GameObject)_variantPrefabField.value, _probabilityField.value);
            
            _addButton.SetEnabled(false);
            _addButton.text = "Added";
        }
    }
}