using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectionRenderSet : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage renderTarget;
    [SerializeField] private Transform renderObjectParent;

    private ShapeObject shapeObject;
    
    void Start()
    {
        cam.targetTexture = renderTexture;
        renderTarget.texture = renderTexture;
        cam.gameObject.SetActive(true);
    }

    public void SetShape(Dictionary<Vector3Int, CellData> shapeDefinition)
    {
        if (shapeObject != null)
            Destroy(shapeObject.gameObject);
        
        shapeObject = ShapeObjectGenerator.Instance.GenerateCentered(shapeDefinition);
        shapeObject.transform.SetParent(renderObjectParent);
        shapeObject.transform.position = renderObjectParent.position;
    }
}
