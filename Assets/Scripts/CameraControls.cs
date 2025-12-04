using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    public Vector3 CurrentRotationEulers => _rotationEulers[_currentRotationIndex];
    
    private Tween _rotationTween;
    private Tween _heightAdjustmentTween;
    private Vector3[] _rotationEulers;
    private int _currentRotationIndex;
    private float _startHeight;

    private void Start()
    {
        _startHeight = transform.position.y;
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
        playerInput.OnCameraTurn += Rotate;
    }
    
    private void OnDisable()
    {
        playerInput.OnCameraTurn -= Rotate;
    }

    public void SetNewHeight(float newHeight)
    {
        _heightAdjustmentTween?.Kill();
        
        var pos = transform.position;
        pos.y = newHeight + _startHeight;
        _heightAdjustmentTween = transform.DOMove(pos, 0.3f).SetEase(Ease.OutSine);
    }

    private void Rotate(float direction)
    {
        int intDirection = (int)-direction;
        // if (_rotationTween != null && _rotationTween.IsActive())
        //     return;
        _rotationTween?.Kill();
        
        _currentRotationIndex = (_currentRotationIndex + intDirection) % _rotationEulers.Length;
        if (_currentRotationIndex < 0)
            _currentRotationIndex += _rotationEulers.Length;
        
        _rotationTween = transform.DORotate(_rotationEulers[_currentRotationIndex], 0.7f).SetEase(Ease.OutSine);
    }
}
