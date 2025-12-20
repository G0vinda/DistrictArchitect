using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectionRenderSet : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage renderTarget;
    [SerializeField] private Transform renderObjectParent;

    public ShapeObject ShapeObject { get; private set; }

    private void Start()
    {
        cam.targetTexture = renderTexture;
        renderTarget.texture = renderTexture;
        cam.gameObject.SetActive(true);
    }

    public void SetShape(Dictionary<Vector3Int, CellData> shapeDefinition)
    {
        if (ShapeObject != null)
            Destroy(ShapeObject.gameObject);
        
        ShapeObject = ShapeObjectGenerator.Instance.GenerateCentered(shapeDefinition);
        ShapeObject.transform.SetParent(renderObjectParent);
        ShapeObject.transform.position = renderObjectParent.position;
    }
}
