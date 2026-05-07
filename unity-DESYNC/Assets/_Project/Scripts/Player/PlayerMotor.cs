using Desync.Core;
using Desync.World.Graph.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                _controller.enabled = false;
                return;
            }

            // Try immediately (works if gameplay scene already loaded).
            // Also subscribe to sceneLoaded for the Bootstrap→House_Prototype transition
            // where NSC doesn't exist yet at spawn time.
            if (!TryBindLocalStreamingContext())
                SceneManager.sceneLoaded += OnSceneLoadedRetryBind;
        }

        public override void OnNetworkDespawn()
        {
            if (!IsOwner) return;

            SceneManager.sceneLoaded -= OnSceneLoadedRetryBind;

            var nsc = FindAnyObjectByType<NodeStreamingController>();
            if (nsc != null)
                nsc.BindLocalPlayer(null, null);
        }

        private void OnSceneLoadedRetryBind(Scene scene, LoadSceneMode mode)
        {
            if (TryBindLocalStreamingContext())
                SceneManager.sceneLoaded -= OnSceneLoadedRetryBind;
        }

        private bool TryBindLocalStreamingContext()
        {
            var nsc = FindAnyObjectByType<NodeStreamingController>();
            if (nsc == null) return false;

            var tracker = GetComponent<PlayerNodeTracker>();
            var cam = GetComponentInChildren<Camera>();
            if (tracker == null || cam == null)
            {
                Debug.LogWarning($"[PlayerMotor] Cannot bind streaming context: tracker={tracker != null}, camera={cam != null}. Check PF_Player prefab.", this);
                return false;
            }
            nsc.BindLocalPlayer(tracker, cam);
            return true;
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
