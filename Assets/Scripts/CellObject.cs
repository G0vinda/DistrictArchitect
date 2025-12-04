using UnityEngine;

public class CellObject : MonoBehaviour
{
    [field: SerializeField] public Collider Collider { get; private set; }
    [SerializeField] private MeshRenderer meshRenderer;
    
    private static readonly int ALPHA = Shader.PropertyToID("_Alpha");
    private static readonly int WHITE_BLEND = Shader.PropertyToID("_WhiteBlend");

    public CellData CellData
    {
        get => _cellData;
        set
        {
            _cellData = value;
            meshRenderer.material = _cellData.Material;
        }
    }
    
    private CellData _cellData;

    public void SetAlpha(float alpha)
    {
        meshRenderer.material.SetFloat(ALPHA, alpha);
    }

    public void SetWhiteBlend(float whiteBlend)
    {
        meshRenderer.material.SetFloat(WHITE_BLEND, whiteBlend);
    }
}