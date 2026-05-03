using Desync.Core;
using UnityEngine;

namespace Desync.Audio
{
    public class AmbientAudioManager : MonoBehaviour
    {
        [SerializeField] private GameplaySettings settings;
        [SerializeField] private AudioClip droneClip;
        [SerializeField] private AudioClip[] oneShotClips;

        private AudioSource _droneSource;
        private float _nextOneShotTime;

        private static AmbientAudioManager _instance;

        private void Awake()
        {
            if (_instance != null) { Destroy(gameObject); return; }
            _instance = this;
        }

        private void Start()
        {
            _droneSource = gameObject.AddComponent<AudioSource>();
            _droneSource.clip = droneClip != null ? droneClip : CreatePlaceholderDrone();
            _droneSource.loop = true;
            _droneSource.volume = settings != null ? settings.ambientDroneVolume : 0.15f;
            _droneSource.spatialBlend = 0f; // 2D — always audible
            _droneSource.Play();

            ScheduleNextOneShot();
        }

        private void Update()
        {
            if (Time.time < _nextOneShotTime) return;

            PlayRandomOneShot();
            ScheduleNextOneShot();
        }

        private void ScheduleNextOneShot()
        {
            float min = settings != null ? settings.ambientOneShotMinInterval : 15f;
            float max = settings != null ? settings.ambientOneShotMaxInterval : 45f;
            _nextOneShotTime = Time.time + Random.Range(min, max);
        }

        private void PlayRandomOneShot()
        {
            var listener = FindAnyObjectByType<AudioListener>();
            if (listener == null) return;

            Vector3 playerPos = listener.transform.position;
            float range = settings != null ? settings.ambientOneShotRange : 15f;

            Vector3 offset = Random.insideUnitSphere * range;
            offset.y = Mathf.Clamp(offset.y, -2f, 2f);
            Vector3 soundPos = playerPos + offset;

            AudioClip clip;
            if (oneShotClips != null && oneShotClips.Length > 0)
                clip = oneShotClips[Random.Range(0, oneShotClips.Length)];
            else
                clip = CreatePlaceholderOneShot();

            float volume = settings != null ? settings.ambientOneShotVolume : 0.3f;
            AudioSource.PlayClipAtPoint(clip, soundPos, volume);
        }

        private static AudioClip CreatePlaceholderDrone()
        {
            int sampleRate = 44100;
            int length = sampleRate * 3;
            var clip = AudioClip.Create("PlaceholderDrone", length, 1, sampleRate, false);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 55f * t) * 0.3f
                         + Mathf.Sin(2f * Mathf.PI * 82f * t) * 0.2f
                         + Mathf.Sin(2f * Mathf.PI * 110f * t) * 0.1f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreatePlaceholderOneShot()
        {
            int sampleRate = 44100;
            int length = sampleRate;
            var clip = AudioClip.Create("PlaceholderOneShot", length, 1, sampleRate, false);
            float[] data = new float[length];
            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-4f * t);
                data[i] = envelope * (Mathf.Sin(2f * Mathf.PI * 80f * t) * 0.5f
                         + (Random.value * 2f - 1f) * 0.2f);
            }
            clip.SetData(data, 0);
            return clip;
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }
    }
}
