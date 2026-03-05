using System;
using UnityEngine;

namespace WFC.TestScene
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private CameraControlInput input;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float rotationSpeed;
        [SerializeField] private float zoomLerpStep;
        [SerializeField] private float maxZoomForwardDistance;
        [SerializeField] private float maxZoomBackDistance;
    
        private Vector3 _desiredPosition;
        private Vector3 _desiredRotationEulers;
        private float _desiredZoomLerpValue;
        private float _zoomLerpValue;
        private Vector3 _closestCameraPosition;
        private Vector3 _farthestCameraPosition;

        private void OnEnable()
        {
            input.ZoomInputRegistered += HandleChangedZoomInput;
        }

        void Start()
        {
            _desiredPosition = transform.position;
            _desiredRotationEulers = transform.rotation.eulerAngles;
            _closestCameraPosition = cameraTransform.localPosition + cameraTransform.forward * maxZoomForwardDistance;
            _farthestCameraPosition = cameraTransform.localPosition + cameraTransform.forward * -maxZoomBackDistance;
            _zoomLerpValue = 0.2f;
            _desiredZoomLerpValue = _zoomLerpValue;
        }
        
        void Update()
        {
            _desiredPosition += (transform.right * input.MovementInput.x + transform.forward * input.MovementInput.y) * (moveSpeed * Time.deltaTime);
            _desiredRotationEulers.y += input.RotationInput * rotationSpeed * Time.deltaTime;
            
            transform.position = Vector3.Lerp(transform.position, _desiredPosition, Time.deltaTime * 10);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(_desiredRotationEulers), Time.deltaTime * 10);
            
            _zoomLerpValue = Mathf.Lerp(_zoomLerpValue, _desiredZoomLerpValue, Time.deltaTime * zoomLerpStep * 16);
            cameraTransform.localPosition = Vector3.Lerp(_farthestCameraPosition, _closestCameraPosition, _zoomLerpValue);
        }
        
        private void HandleChangedZoomInput(float zoomInputChange)
        {
            _desiredZoomLerpValue = Mathf.Clamp01(_desiredZoomLerpValue + zoomInputChange * zoomLerpStep);
        }
    }
}
