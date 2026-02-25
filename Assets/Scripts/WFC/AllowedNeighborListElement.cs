using UnityEngine;
using UnityEngine.UIElements;
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
                var previewTexture = EditorUtils.GetPrefabPreviewTexture(prefab, 50);
                var previewImage = new Image()
                {
                    name = "PreviewImage",
                    style =
                    {
                        width = previewTexture.width,
                        height = previewTexture.height,
                    },
                    image = previewTexture,
                    scaleMode = ScaleMode.StretchToFill,
                    tintColor = Color.white
                };
                previewImageContainer.Add(previewImage);
            }
            
            prefabNameLabel.text = elementName;
            
            Add(previewImageContainer);
            Add(prefabNameLabel);
            Add(allowanceSwitch);
        }
    }
}