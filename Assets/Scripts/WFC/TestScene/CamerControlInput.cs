using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WFC.TestScene
{
    [CreateAssetMenu(fileName = "CameraControlInput", menuName = "CameraControlInput")]
    public class CameraControlInput : ScriptableObject, WfcTestSceneCamera.IDefaultActions
    {
        public Vector2 MovementInput { get; private set; }
        public float RotationInput { get; private set; }

        public Action<float> ZoomInputRegistered;

        private WfcTestSceneCamera _cameraControls;

        private void OnEnable()
        {
            if (_cameraControls == null)
            {
                _cameraControls = new WfcTestSceneCamera();
                _cameraControls.Default.SetCallbacks(this);
            }
            MovementInput = Vector2.zero;
            RotationInput = 0f;
            _cameraControls.Enable();
        }

        private void OnDisable()
        {
            _cameraControls.Disable();
        }

        public void OnCameraRotate(InputAction.CallbackContext context)
        {
            RotationInput = -context.ReadValue<float>();
        }

        public void OnCameraMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnZoom(InputAction.CallbackContext context)
        {
            if (!context.performed)
                return;
            
            ZoomInputRegistered?.Invoke(-context.ReadValue<float>());
        }
    }
}