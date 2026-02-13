using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    [CustomEditor(typeof(WfcConfig))]
    public class WfcConfigEditor : Editor
    {
        [SerializeField] private VisualTreeAsset visualTree;

        private Button _testButton;
        
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            visualTree.CloneTree(root);
            
            _testButton = root.Q<Button>("TestButton");
            _testButton.RegisterCallback<ClickEvent>(OnButtonClick);
            
            return root;
        }

        private void OnButtonClick(ClickEvent evt)
        {
            var popUp = EditorWindow.CreateInstance<SubBlockAddWindow>();
            popUp.WfcConfig = (WfcConfig)target;
            popUp.Show();
        }
    }
}
