using System;
using UnityEngine.UIElements;

namespace WFC
{
    [UxmlElement]
    public partial class Switch : VisualElement
    {
        [UxmlAttribute]
        public bool Value
        {
            get => _value;
            set => SetValue(value);
        }
        
        public Action<bool> ValueChanged { get; set; }

        private VisualElement _border;
        private VisualElement _control;
        private bool _value;

        public Switch()
        {
            _border = new VisualElement();
            _border.name = "Border";
            _border.AddToClassList("border");
            
            _control = new VisualElement();
            _control.name = "Control";
            _control.AddToClassList("control");
            
            Add(_border);
            _border.Add(_control);
            
            RegisterCallback<MouseDownEvent>(evt => Value = !Value);
        }

        private void SetValue(bool value)
        {
            _value = value;
            ValueChanged?.Invoke(value);
            _border.EnableInClassList("border-on", value);
            _control.EnableInClassList("control-on", value);
        }
    }
}
