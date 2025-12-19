using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem.Composites;

public class CameraControls : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Camera childCam;

    public Vector3 CurrentRotationEulers => _rotationEulers[_currentRotationIndex];
    
    private Tween _rotationTween;
    private Tween _heightAdjustmentTween;
    private Vector3[] _rotationEulers;
    private int _currentRotationIndex;
    private float _startHeight;
    private Coroutine _dragCoroutine;

    private void Start()
    {
        _startHeight = transform.position.y;
        _rotationEulers = new[]
        {
            Vector3.zero,
            new Vector3(0, 90, 0),
            new Vector3(0, 180, 0),
            new Vector3(0, 270, 0),
        };
        
    }

    private void OnEnable()
    {
        playerInput.OnCameraTurn += Rotate;
        playerInput.OnCameraRotationStarted += OnCameraRotationStarted;
        playerInput.OnCameraRotationStopped += OnCameraRotationReleased;
    }
    
    private void OnDisable()
    {
        playerInput.OnCameraTurn -= Rotate;
        playerInput.OnCameraRotationStarted -= OnCameraRotationStarted;
        playerInput.OnCameraRotationStopped -= OnCameraRotationReleased;
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
        if (Quaternion.Angle(transform.rotation, Quaternion.Euler(_rotationEulers[_currentRotationIndex])) > 91f)
            return;
        
        // if (_rotationTween != null && _rotationTween.IsActive())
        //     return;
        _rotationTween?.Kill();
        
        _currentRotationIndex = (_currentRotationIndex + intDirection) % _rotationEulers.Length;
        if (_currentRotationIndex < 0)
            _currentRotationIndex += _rotationEulers.Length;
        
        _rotationTween = transform.DORotate(_rotationEulers[_currentRotationIndex], 0.7f).SetEase(Ease.OutSine);
    }

    private void OnCameraRotationStarted()
    {
        _dragCoroutine = StartCoroutine(Drag());
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnCameraRotationReleased()
    {
        StopCoroutine(_dragCoroutine);

        var currentEulerY = transform.eulerAngles.y;
        currentEulerY %= 360;
        if (currentEulerY < 0)
            currentEulerY += 360;
        var closestFixedRotation = _rotationEulers.OrderBy(euler =>
            Mathf.Min(Mathf.Abs(currentEulerY - euler.y), Mathf.Abs(currentEulerY - 360 - euler.y))).First();
        _rotationTween?.Kill();
        _rotationTween = _rotationTween = transform.DORotate(closestFixedRotation, 0.2f).SetEase(Ease.OutSine);
        _currentRotationIndex = Array.IndexOf(_rotationEulers, closestFixedRotation);
        Cursor.lockState = CursorLockMode.None;
    }

    private IEnumerator Drag()
    {
        var maxVerticalRotationAngle = 75f;
        var minVerticalRotationAngle = 15f;
        
        while (true)
        {
            transform.Rotate(Vector3.up, playerInput.MouseDelta.x * 20f * Time.deltaTime);
            var currentVerticalAngle = childCam.transform.eulerAngles.x;
            var newAngle = Mathf.Clamp(currentVerticalAngle -playerInput.MouseDelta.y * 20f * Time.deltaTime, minVerticalRotationAngle, maxVerticalRotationAngle);
            var angleChange = newAngle - currentVerticalAngle;
            childCam.transform.RotateAround(transform.position, transform.right, angleChange);
            yield return null;
        }
    }
}
