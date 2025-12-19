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
    private float _timeOnSecondaryPressStart;
    private bool _cancelOnRelease;
    private bool _isSecondaryDragging;
    private bool _isSecondaryPressed;
    
    private const float DRAG_DISTANCE_THRESHOLD = 15f;
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
        if (!context.performed || _isSecondaryDragging) 
            return;
        
        var newMousePosition = context.ReadValue<Vector2>();
        MousePosition = newMousePosition;

        if (_isSecondaryPressed && (newMousePosition - _mousePositionOnSecondaryPressStart).magnitude > DRAG_DISTANCE_THRESHOLD)
        {
            _isSecondaryDragging = true;
            OnCameraRotationStarted?.Invoke();
            return;
        }
        
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
        if (!context.performed  || _isSecondaryDragging)
            return;

        var turnDirection = context.ReadValue<float>();
        OnHorizontalTurn?.Invoke(turnDirection);
    }

    public void OnTurnVertically(InputAction.CallbackContext context)
    {
        if (!context.performed  || _isSecondaryDragging)
            return;

        var turnDirection = context.ReadValue<float>();
        OnVerticalTurn?.Invoke(turnDirection);
    }

    public void OnChangeFloor(InputAction.CallbackContext context)
    {
        if (!context.performed || _isSecondaryDragging)
            return;
        
        OnFloorChanged?.Invoke(context.ReadValue<float>());
    }

    public void OnTurnCameraHorizontally(InputAction.CallbackContext context)
    {
        if (!context.performed  || _isSecondaryDragging)
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
    }

    public void OnSecondaryButton(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (context.ReadValueAsButton())
        {
            _mousePositionOnSecondaryPressStart = MousePosition;
            _isSecondaryPressed = true;
            _cancelOnRelease = true;
        }
        else if (_isSecondaryDragging)
        {
            OnCameraRotationStopped?.Invoke();
            _isSecondaryPressed = false;
            _isSecondaryDragging = false;
        }
        else
        {
            OnCancelled?.Invoke();
            _isSecondaryPressed = false;
        }
    }
}
