using UnityEngine;
using UnityEngine.Rendering;

public class CellObject : MonoBehaviour
{
    [field: SerializeField] public Collider Collider { get; private set; }
    [SerializeField] private MeshRenderer meshRenderer;
    
    private static readonly int ALPHA = Shader.PropertyToID("_Alpha");
    private static readonly int WHITE_BLEND = Shader.PropertyToID("_WhiteBlend");
    private static readonly int DISABLED = Shader.PropertyToID("_Disabled");

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

    public void SetDisabled(bool disabled)
    {
        meshRenderer.material.SetFloat(DISABLED, disabled ? 1 : 0);
    }

    public void SetCastShadows(bool castShadows)
    {
        meshRenderer.shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
    }
}