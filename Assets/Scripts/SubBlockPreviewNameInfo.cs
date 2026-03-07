using TMPro;
using UnityEngine;

public class SubBlockPreviewNameInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI nameTextMesh;
    [SerializeField] Canvas canvas;

    public void SetCamera(Camera camera)
    {
        canvas.worldCamera = camera;
    }

    public void SetName(string previewName)
    {
        nameTextMesh.text = previewName;
    }
}
