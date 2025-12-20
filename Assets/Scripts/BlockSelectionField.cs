using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BlockSelectionField : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] internal BlockSelectionRenderSet renderSet;
    [SerializeField] private UnityEvent<BlockSelectionField> onBlockSelected;
    [SerializeField] private UnityEvent onHovered;
    
    private Tween scaleTween;
    
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        scaleTween?.Kill();
        
        transform.localScale = Vector3.one;
        scaleTween = transform.DOScale(Vector3.one * 1.15f, 0.15f).SetEase(Ease.OutSine);
        onHovered?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scaleTween?.Kill();
        scaleTween = transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutSine);
    }
}