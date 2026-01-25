using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectionRenderSet : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage renderTarget;
    [SerializeField] private Transform renderObjectParent;

    public Shape Shape { get; private set; }

    private void Start()
    {
        cam.targetTexture = renderTexture;
        renderTarget.texture = renderTexture;
        cam.gameObject.SetActive(true);
    }

    public void SetShape(Dictionary<Vector3Int, Building> shapeDefinition)
    {
        if (Shape != null)
            Destroy(Shape.gameObject);
        
        Shape = ShapeGenerator.Instance.GenerateCentered(shapeDefinition);
        Shape.transform.SetParent(renderObjectParent);
        Shape.transform.position = renderObjectParent.position;
    }
}
