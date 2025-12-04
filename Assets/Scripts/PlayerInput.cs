using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "PlayerInput", order = 1)]
public class PlayerInput : ScriptableObject, Controls.IPlayerActions
{
    public Vector2 MousePosition { get; private set; }
    
    public Action<Vector2> OnMousePositionChanged;
    public Action OnMouseClicked;
    public Action<float> OnHorizontalTurn;
    public Action<float> OnVerticalTurn;
    public Action<float> OnCameraTurn;
    public Action<float> OnFloorChanged;
    public Action OnRightClicked;
    
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

    public void OnTurnHorizontally(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var turnDirection = context.ReadValue<float>();
        OnHorizontalTurn?.Invoke(turnDirection);
    }

    public void OnTurnVertically(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        var turnDirection = context.ReadValue<float>();
        OnVerticalTurn?.Invoke(turnDirection);
    }

    public void OnChangeFloor(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        OnFloorChanged?.Invoke(-context.ReadValue<float>());
    }

    public void OnTurnCameraHorizontally(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        var turnDirection = context.ReadValue<float>();
        OnCameraTurn?.Invoke(turnDirection);
        
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnRighClick(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        OnRightClicked?.Invoke();
    }
}
