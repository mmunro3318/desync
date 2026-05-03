using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Desync.Player
{
    public class PlayerInputRouter : NetworkBehaviour
    {
        private PlayerInputActions _inputActions;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool SprintPressed { get; private set; }
        public event Action FlashlightToggled;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = false;
                return;
            }

            _inputActions = new PlayerInputActions();
            _inputActions.Player.ToggleFlashlight.performed += OnFlashlightToggle;
            _inputActions.Player.Enable();
        }

        public override void OnNetworkDespawn()
        {
            if (_inputActions != null)
            {
                _inputActions.Player.ToggleFlashlight.performed -= OnFlashlightToggle;
                _inputActions.Player.Disable();
                _inputActions.Dispose();
                _inputActions = null;
            }
        }

        private void Update()
        {
            if (!IsOwner || _inputActions == null) return;

            MoveInput = _inputActions.Player.Move.ReadValue<Vector2>();
            LookInput = _inputActions.Player.Look.ReadValue<Vector2>();
            SprintPressed = _inputActions.Player.Sprint.IsPressed();
        }

        private void OnFlashlightToggle(InputAction.CallbackContext ctx)
        {
            FlashlightToggled?.Invoke();
        }
    }
}
