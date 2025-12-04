using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BlockSelectionField : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private BlockSelectionRenderSet renderSet;
    [SerializeField] private UnityEvent<BlockSelectionField> onBlockSelected;
    
    public List<Vector3Int> BlockPositions { get; private set; }
    public Material BlockMaterial { get; private set; }

    public void SetBlockShape(List<Vector3Int> blockPositions, Material blockMaterial)
    {
        BlockPositions = blockPositions;
        BlockMaterial = blockMaterial;
        renderSet.SetShape(BlockPositions, BlockMaterial);
    }

    public void SetHighlight(bool highlight)
    {
        canvasGroup.alpha = highlight ? 0.6f : 1.0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onBlockSelected?.Invoke(this);
    }
}