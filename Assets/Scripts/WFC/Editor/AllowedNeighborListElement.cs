using UnityEngine;
using UnityEngine.UIElements;
using WFC.Editor;
using Label = UnityEngine.UIElements.Label;

namespace WFC
{
    [UxmlElement]
    public partial class AllowedNeighborListElement : VisualElement
    {
        public AllowedNeighborListElement()
        { }
        
        public AllowedNeighborListElement(string elementName, GameObject prefab)
        {
            var previewImageContainer = new VisualElement { name = "PreviewImageContainer" };
            var prefabNameLabel = new Label { name = "PrefabNameLabel" };
            var allowanceSwitch = new Switch { name = "AllowanceSwitch" };

            if (prefab)
            {
                var previewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 40);
                previewImageContainer.Add(EditorUtils.GenerateImage(previewTexture));
            }
            
            prefabNameLabel.text = elementName;
            
            Add(previewImageContainer);
            Add(prefabNameLabel);
            Add(allowanceSwitch);
        }
    }
}