using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockSelectionRenderSet : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private RenderTexture renderTexture;
    [SerializeField] private RawImage renderTarget;
    [SerializeField] private MeshFilter renderObject;
    
    public Mesh Mesh { get; private set; }
    
    void Start()
    {
        cam.targetTexture = renderTexture;
        renderTarget.texture = renderTexture;
        cam.gameObject.SetActive(true);
    }

    public void SetShape(List<Vector3Int> blockPositions, Material material)
    {
        Mesh = BlockMeshGenerator.GenerateCenteredMesh(blockPositions);
        renderObject.mesh = Mesh;
        renderObject.GetComponent<MeshRenderer>().material = material;
    }
}
