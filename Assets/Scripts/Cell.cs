using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Cell : MonoBehaviour
{
    [field: SerializeField] public Collider Collider { get; private set; }
    [SerializeField] public MeshRenderer meshRenderer;
    [SerializeField] private ParticleSystem destroyVfxPrefab;
    
    private static readonly int ALPHA = Shader.PropertyToID("_Alpha");
    private static readonly int WHITE_BLEND = Shader.PropertyToID("_WhiteBlend");
    private static readonly int DISABLED = Shader.PropertyToID("_Disabled");

    private int value = 0;

    public static event Action<Vector3> CellObjectDestroyed;
    public static event Action<Vector3> CellObjectScored;
    
    [SerializeField] public Building Building;

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

    public void IncreaseValue(int increaseValue)
    {
        value += increaseValue;
        CellObjectScored?.Invoke(transform.position);
    }

    public int GetValue()
    {
        return value;
    }
}