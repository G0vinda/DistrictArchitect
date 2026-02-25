using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    public class SubBlockDeleteConfirmWindow : EditorWindow
    {
        public Action ConfirmedButtonPressed;
        
        [SerializeField] private VisualTreeAsset visualTreeAsset;

        private Label _textLabel;
        private string _subBlockName;

        public void SetSubBlockName(string name)
        {
            _subBlockName = name;
            if (_textLabel == null)
                return;
            
            _textLabel.text = $"Do you really want to delete {name}?";
        }

        private void CreateGUI()
        {
            visualTreeAsset.CloneTree(rootVisualElement);
            _textLabel = rootVisualElement.Q<Label>("TextLabel");
            _textLabel.text = $"Do you really want to delete {_subBlockName}?";

            var cancelButton = rootVisualElement.Q<Button>("CancelButton");
            cancelButton.RegisterCallback<ClickEvent>((_) => Close());
            
            var confirmButton = rootVisualElement.Q<Button>("ConfirmButton");
            confirmButton.RegisterCallback<ClickEvent>((_) =>
            {
                ConfirmedButtonPressed?.Invoke();
                Close();
            });
        }
    }
}