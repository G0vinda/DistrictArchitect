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

        private Button _addSubBlockButton;
        private Label _subBlockCountText;
        private VisualElement _root;
        
        public override VisualElement CreateInspectorGUI()
        {
            _root = new VisualElement();

            visualTree.CloneTree(_root);
            
            _addSubBlockButton = _root.Q<Button>("AddSubBlockButton");
            _addSubBlockButton.RegisterCallback<ClickEvent>(OnButtonClick);

            _subBlockCountText = _root.Q<Label>("SubBlockCountText");
            _subBlockCountText.text = $"SubBlocks added: {((WfcConfig)target).GetSubBlockCount()}";
            
            return _root;
        }

        public override void OnInspectorGUI()
        {
            _subBlockCountText = _root.Q<Label>("SubBlockCountText");
            _subBlockCountText.text = $"SubBlocks added: {((WfcConfig)target).GetSubBlockCount()}";
        }

        private void OnButtonClick(ClickEvent evt)
        {
            var popUp = CreateInstance<SubBlockAddWindow>();
            popUp.WfcConfig = (WfcConfig)target;
            popUp.Show();
        }
    }
}
