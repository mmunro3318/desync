using Desync.Core;
using Desync.Player;
using Unity.Netcode;
using UnityEngine;

namespace Desync.Audio
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerInputRouter))]
    public class FootstepAudio : NetworkBehaviour
    {
        [SerializeField] private GameplaySettings settings;
        [SerializeField] private AudioClip[] footstepClips;

        private CharacterController _controller;
        private PlayerInputRouter _input;
        private AudioSource _localSource;
        private float _stepTimer;
        private AudioClip _placeholderClip;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerInputRouter>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                _localSource = gameObject.AddComponent<AudioSource>();
                _localSource.spatialBlend = 0f; // 2D
                _localSource.playOnAwake = false;
                _localSource.volume = settings != null ? settings.footstepVolume : 0.4f;
            }
        }

        private void Update()
        {
            if (!IsOwner) return;

            Vector3 horizontalVelocity = _controller.velocity;
            horizontalVelocity.y = 0f;
            float speed = horizontalVelocity.magnitude;

            if (speed < 0.1f || !_controller.isGrounded)
            {
                _stepTimer = 0f;
                return;
            }

            float interval = _input.SprintPressed
                ? (settings != null ? settings.footstepSprintInterval : 0.33f)
                : (settings != null ? settings.footstepWalkInterval : 0.5f);

            _stepTimer += Time.deltaTime;
            if (_stepTimer >= interval)
            {
                _stepTimer -= interval;
                PlayFootstepLocal();
                PlayFootstepRemoteClientRpc();
            }
        }

        private void PlayFootstepLocal()
        {
            AudioClip clip = GetFootstepClip();
            if (clip != null && _localSource != null)
            {
                _localSource.pitch = Random.Range(0.9f, 1.1f);
                _localSource.PlayOneShot(clip);
            }
        }

        [ClientRpc]
        private void PlayFootstepRemoteClientRpc()
        {
            if (IsOwner) return;

            AudioClip clip = GetFootstepClip();
            if (clip == null) return;

            float volume = settings != null ? settings.footstepVolume : 0.4f;
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }

        private AudioClip GetFootstepClip()
        {
            if (footstepClips != null && footstepClips.Length > 0)
                return footstepClips[Random.Range(0, footstepClips.Length)];

            if (_placeholderClip == null)
                _placeholderClip = CreatePlaceholderFootstep();
            return _placeholderClip;
        }

        private static AudioClip CreatePlaceholderFootstep()
        {
            int sampleRate = 44100;
            int length = sampleRate / 10; // 0.1 seconds
            var clip = AudioClip.Create("PlaceholderFootstep", length, 1, sampleRate, false);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-30f * t);
                data[i] = envelope * (Mathf.Sin(2f * Mathf.PI * 150f * t) * 0.6f
                         + (Random.value * 2f - 1f) * 0.3f);
            }
            clip.SetData(data, 0);
            return clip;
        }
    }
}
