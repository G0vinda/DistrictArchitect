using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    public class SubBlockAddWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset visualTree;
        
        public WfcConfig WfcConfig { get; set; }

        private Button _closeButton;
        private ObjectField _prefabField;
        private Vector3IntField _positionField;
        
        private void CreateGUI()
        {
            visualTree.CloneTree(rootVisualElement);
            
            _closeButton = rootVisualElement.Q<Button>("AddButton");
            _closeButton.RegisterCallback<ClickEvent>(AddButtonPressed);
            
            _prefabField = rootVisualElement.Q<ObjectField>("PrefabField");
            _positionField = rootVisualElement.Q<Vector3IntField>("PositionField");
        }

        private void AddButtonPressed(ClickEvent evt)
        {
            WfcConfig.AddSubBlock(_positionField.value, (GameObject)_prefabField.value);
            Close();
        }
    }
}