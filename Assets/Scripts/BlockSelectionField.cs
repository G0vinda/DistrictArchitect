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
    
    public Dictionary<Vector3Int, CellData> ShapeDefinition { get; private set; }

    public void SetShapeDefinition(Dictionary<Vector3Int, CellData> shapeDefinition)
    {
        ShapeDefinition = shapeDefinition;
        renderSet.SetShape(ShapeDefinition);
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