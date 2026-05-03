using Desync.Core;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Desync.Player
{
    [RequireComponent(typeof(PlayerInputRouter))]
    public class PlayerLook : NetworkBehaviour
    {
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private GameplaySettings settings;

        private PlayerInputRouter _input;
        private float _pitch;
        private Camera _ownerCamera;

        private readonly NetworkVariable<float> _networkedPitch = new(0f,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private void Awake()
        {
            _input = GetComponent<PlayerInputRouter>();
        }

        public override void OnNetworkSpawn()
        {
            if (cameraRoot == null)
            {
                Debug.LogError($"[PlayerLook] cameraRoot is not assigned on {gameObject.name}.", this);
                return;
            }

            if (IsOwner)
            {
                // Enable owner camera + audio listener
                _ownerCamera = cameraRoot.GetComponentInChildren<Camera>();
                if (_ownerCamera != null)
                {
                    _ownerCamera.enabled = true;
                    _ownerCamera.depth = 1;
                }

                var listener = cameraRoot.GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = true;

                // The player spawns in Bootstrap before the gameplay scene loads.
                // We must listen for scene loads to destroy the gameplay scene's
                // Main Camera, since it doesn't exist yet at this point.
                SceneManager.sceneLoaded += OnSceneLoaded;
                DestroySceneCamera(); // Also try now in case scene is already loaded

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                // CRITICAL NGO PATTERN: disable camera + audio listener on non-owner
                var cam = cameraRoot.GetComponentInChildren<Camera>();
                if (cam != null) cam.enabled = false;

                var listener = cameraRoot.GetComponentInChildren<AudioListener>();
                if (listener != null) listener.enabled = false;

                // Sync visual pitch from network variable
                _networkedPitch.OnValueChanged += OnPitchChanged;
                ApplyPitch(_networkedPitch.Value);
            }
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                _networkedPitch.OnValueChanged -= OnPitchChanged;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (IsOwner)
                DestroySceneCamera();
        }

        private void DestroySceneCamera()
        {
            var sceneCam = GameObject.FindWithTag("MainCamera");
            if (sceneCam != null && sceneCam != _ownerCamera?.gameObject)
                Destroy(sceneCam);
        }

        private void Update()
        {
            if (!IsOwner) return;

            Vector2 look = _input.LookInput;

            // Yaw: rotate player transform
            transform.Rotate(Vector3.up, look.x * settings.mouseSensitivity);

            // Pitch: rotate camera root
            _pitch -= look.y * settings.mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -settings.pitchClamp, settings.pitchClamp);

            cameraRoot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            _networkedPitch.Value = _pitch;
        }

        private void OnPitchChanged(float oldVal, float newVal)
        {
            ApplyPitch(newVal);
        }

        private void ApplyPitch(float pitch)
        {
            if (cameraRoot != null)
                cameraRoot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }
}
