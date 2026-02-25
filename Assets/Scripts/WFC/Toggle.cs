using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace WFC
{
    [UxmlElement] 
    public partial class Toggle : VisualElement
    {
        public bool Value
        {
            get => _value;
            set => SetValue(value);
        }
    
        public Action<bool> ValueChangedOnClick { get; set; }
        public Action DoubleClicked { get; set; }

        private VisualElement _background;
        private Label _label;
        private bool _value = true;

        public Toggle()
        {
            _background = new VisualElement();
            _background.name = "Background";
            _background.AddToClassList("background");
            _background.AddToClassList("background-on");
            
            _label = new Label();
            _label.name = "TextLabel";
            _label.text = "Text";
        
            Add(_background);
            _background.Add(_label);
        
            RegisterCallback<ClickEvent>(evt =>
            {
                if (evt.clickCount >= 2)
                {
                    DoubleClicked?.Invoke();
                    return;
                }
                
                Value = !Value;
                ValueChangedOnClick?.Invoke(Value);
            });
        }

        public void SetText(string text)
        {
            _label.text = text;
        }

        private void SetValue(bool value)
        {
            _value = value;
            _background.EnableInClassList("background-on", value);
        }
    }
}