using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CellObject : MonoBehaviour
{
    [field: SerializeField] public Collider Collider { get; private set; }
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem destroyVfxPrefab;
    
    private static readonly int ALPHA = Shader.PropertyToID("_Alpha");
    private static readonly int WHITE_BLEND = Shader.PropertyToID("_WhiteBlend");
    private static readonly int DISABLED = Shader.PropertyToID("_Disabled");

    public static event Action<Vector3> CellObjectDestroyed;

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

    public void DestroyWithVfx()
    {
        var vfx = Instantiate(destroyVfxPrefab, transform.position, Quaternion.Euler(-90, 0, 0));
        var vfxRenderer = vfx.GetComponent<ParticleSystemRenderer>();
        vfxRenderer.sharedMaterial = meshRenderer.material;
        vfx.Play();
        CellObjectDestroyed?.Invoke(transform.position);
        Destroy(gameObject);
    }
}