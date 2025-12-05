using System;
using PlasticGui.WorkspaceWindow.Diff;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FinishPlaneVisual : MonoBehaviour
{
    [SerializeField] private float maxAlpha = 0.7f;
    
    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void FitTransparencyToCameraYLevel(int cameraYLevel)
    {
        var heightValue = Mathf.InverseLerp(0, 4, cameraYLevel);
        _meshRenderer.material.SetFloat("_Alpha", Mathf.Lerp(0, maxAlpha, heightValue));
    }
}
