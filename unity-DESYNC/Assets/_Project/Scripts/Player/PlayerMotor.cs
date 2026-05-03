using Desync.Core;
using Unity.Netcode;
using UnityEngine;

namespace Desync.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputRouter))]
    public class PlayerMotor : NetworkBehaviour
    {
        [SerializeField] private GameplaySettings settings;

        private CharacterController _controller;
        private PlayerInputRouter _input;
        private float _verticalVelocity;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputRouter>();
            ApplyCapsuleSettings();
        }

        private void ApplyCapsuleSettings()
        {
            if (settings == null)
            {
                Debug.LogError($"PlayerMotor on '{name}' has no GameplaySettings assigned; capsule values will keep prefab defaults.", this);
                return;
            }
            _controller.radius = settings.playerRadius;
            _controller.height = settings.playerHeight;
            _controller.stepOffset = settings.playerStepOffset;
            _controller.slopeLimit = settings.playerSlopeLimit;
            _controller.skinWidth = settings.playerSkinWidth;
        }

        private void Update()
        {
            if (!IsOwner) return;

            float speed = _input.SprintPressed ? settings.sprintSpeed : settings.walkSpeed;

            Vector3 move = transform.right * _input.MoveInput.x
                         + transform.forward * _input.MoveInput.y;
            move *= speed;

            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            _verticalVelocity += settings.gravity * Time.deltaTime;
            move.y = _verticalVelocity;

            _controller.Move(move * Time.deltaTime);
        }
    }
}
