using System.Linq;
using UnityEngine.UIElements;

namespace WFC
{
    [UxmlElement]
    public partial class ToggleBar : VisualElement
    {
        public ToggleBar()
        { }

        public Toggle AddToggle(string toggleText)
        {
            var toggle = new Toggle();
            toggle.SetText(toggleText);
            Add(toggle);
            
            for (int i = 0; i < childCount; i++)
            {
                Children().ElementAt(i).EnableInClassList("first", i == 0);
                Children().ElementAt(i).EnableInClassList("last", i == childCount - 1);
            }
            
            return toggle;
        }

        public void SetAllTogglesActive(bool active)
        {
            for (int i = 0; i < childCount; i++)
            {
                SetToggleActive(i, active);
            }
        }

        private void SetToggleActive(int index, bool active)
        {
            ((Toggle)Children().ElementAt(index)).Value = active;
        }
    }
}