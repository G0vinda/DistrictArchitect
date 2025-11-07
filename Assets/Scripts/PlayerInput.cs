using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "PlayerInput", order = 1)]
public class PlayerInput : ScriptableObject, Controls.IPlayerActions
{
    public Vector2 MousePosition { get; private set; }
    
    public Action<Vector2> OnMousePositionChanged;
    public Action OnMouseClicked;
    
    Controls controls;
    
    private void OnEnable()
    {
        if (controls == null)
        {
            controls = new Controls();
            controls.Player.SetCallbacks(this);
        }
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    public void OnMousePosition(InputAction.CallbackContext context)
    {
        if (!context.performed) 
            return;
        
        var newMousePosition = context.ReadValue<Vector2>();
        MousePosition = newMousePosition;
        OnMousePositionChanged?.Invoke(newMousePosition);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        OnMouseClicked?.Invoke();
    }
}
