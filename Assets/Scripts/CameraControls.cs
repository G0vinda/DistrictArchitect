using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    
    private Tween _rotationTween;
    private Vector3[] _rotationEulers;
    private int _currentRotationIndex;

    private void Start()
    {
        _rotationEulers = new[]
        {
            Vector3.zero,
            new Vector3(0, 90, 0),
            new Vector3(0, -180, 0),
            new Vector3(0, -90, 0),
        };
    }

    private void OnEnable()
    {
        playerInput.OnCameraTurnRight += RotateRight;
        playerInput.OnCameraTurnLeft += RotateLeft;
    }
    
    private void OnDisable()
    {
        playerInput.OnCameraTurnRight -= RotateRight;
        playerInput.OnCameraTurnLeft -= RotateLeft;
    }
    
    [ContextMenu("RotateRight")]
    private void RotateRight()
    {
        Rotate(-1);
    }

    [ContextMenu("RotateLeft")]
    private void RotateLeft()
    {
        Rotate(1);
    }

    private void Rotate(int direction)
    {
        if (_rotationTween != null && _rotationTween.IsActive())
            return;
        
        _currentRotationIndex = (_currentRotationIndex + direction) % _rotationEulers.Length;
        if (_currentRotationIndex < 0)
            _currentRotationIndex += _rotationEulers.Length;
        
        _rotationTween = transform.DORotate(_rotationEulers[_currentRotationIndex], 0.7f).SetEase(Ease.InSine);
    }
}
