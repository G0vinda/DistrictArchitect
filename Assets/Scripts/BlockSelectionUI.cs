using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BlockSelectionUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private BuildingBlock buildingBlock;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private UnityEvent<BuildingBlock> onBlockSelected;

    private static Action _uiShouldReset;

    private void OnEnable()
    {
        _uiShouldReset += UnHighlight;
    }

    private void OnDisable()
    {
        _uiShouldReset -= UnHighlight;
    }
    
    private void UnHighlight()
    {
        highlightObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _uiShouldReset?.Invoke();
        highlightObject.SetActive(true);
        onBlockSelected?.Invoke(buildingBlock);
    }
}
