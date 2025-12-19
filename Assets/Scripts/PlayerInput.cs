using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "PlayerInput", menuName = "PlayerInput", order = 1)]
public class PlayerInput : ScriptableObject, Controls.IPlayerActions
{
    public Vector2 MousePosition { get; private set; }
    public Vector2 MouseDelta { get; private set; }
    
    public Action<Vector2> OnMousePositionChanged;
    public Action OnMouseClicked;
    public Action<float> OnHorizontalTurn;
    public Action<float> OnVerticalTurn;
    public Action<float> OnCameraTurn;
    public Action<float> OnFloorChanged;
    public Action OnCameraRotationStarted;
    public Action OnCameraRotationStopped;
    public Action OnCancelled;
    
    private Vector2 _mousePositionOnSecondaryPressStart;
    private Vector2 _mouseDragPosition;
    private float _timeOnSecondaryPressStart;
    private bool _cancelOnRelease;
    private bool _isSecondaryPressed;
    
    private const float DRAG_DISTANCE_THRESHOLD = 100f;
    private const float DRAG_TIME_THRESHOLD = 0.8f;
    
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
        if (!context.performed || _isSecondaryPressed) 
            return;
        
        var newMousePosition = context.ReadValue<Vector2>();
        MousePosition = newMousePosition;
        
        OnMousePositionChanged?.Invoke(newMousePosition);
    }

    public void OnClick(InputAction.CallbackContext context)
    {
        if (!context.performed || _isSecondaryPressed)
            return;
        
        OnMouseClicked?.Invoke();
    }

    public void OnTurnHorizontally(InputAction.CallbackContext context)
    {
        if (!context.performed  || _isSecondaryPressed)
            return;

        var turnDirection = context.ReadValue<float>();
        OnHorizontalTurn?.Invoke(turnDirection);
    }

    public void OnTurnVertically(InputAction.CallbackContext context)
    {
        if (!context.performed  || _isSecondaryPressed)
            return;

        var turnDirection = context.ReadValue<float>();
        OnVerticalTurn?.Invoke(turnDirection);
    }

    public void OnChangeFloor(InputAction.CallbackContext context)
    {
        if (!context.performed || _isSecondaryPressed)
            return;
        
        OnFloorChanged?.Invoke(context.ReadValue<float>());
    }

    public void OnTurnCameraHorizontally(InputAction.CallbackContext context)
    {
        if (!context.performed  || _isSecondaryPressed)
            return;
        var turnDirection = context.ReadValue<float>();
        OnCameraTurn?.Invoke(turnDirection);
    }

    public void OnPause(InputAction.CallbackContext context)
    {
        throw new NotImplementedException();
    }

    public void OnMouseMovement(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;
        
        MouseDelta = context.ReadValue<Vector2>();
        _mouseDragPosition += MouseDelta;
        
        if (_isSecondaryPressed && _cancelOnRelease && (_mouseDragPosition - _mousePositionOnSecondaryPressStart).magnitude > DRAG_DISTANCE_THRESHOLD)
            _cancelOnRelease = false;
    }

    public void OnSecondaryButton(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (context.ReadValueAsButton())
        {
            _mousePositionOnSecondaryPressStart = MousePosition;
            _mouseDragPosition = MousePosition;
            _isSecondaryPressed = true;
            _cancelOnRelease = true;
            OnCameraRotationStarted?.Invoke();
        }
        else
        {
            if (Time.time - _timeOnSecondaryPressStart < DRAG_TIME_THRESHOLD || _cancelOnRelease)
                OnCancelled?.Invoke();
            
            OnCameraRotationStopped?.Invoke();
            _isSecondaryPressed = false;
        }
    }
}
