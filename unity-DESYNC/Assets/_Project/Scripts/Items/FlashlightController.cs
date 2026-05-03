using Desync.Core;
using Desync.Player;
using Unity.Netcode;
using UnityEngine;

namespace Desync.Items
{
    public class FlashlightController : NetworkBehaviour
    {
        [SerializeField] private Light spotLight;
        [SerializeField] private GameplaySettings settings;

        private readonly NetworkVariable<bool> _isOn = new(true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (settings != null && spotLight != null)
            {
                spotLight.range = settings.flashlightRange;
                spotLight.spotAngle = settings.flashlightSpotAngle;
                spotLight.intensity = settings.flashlightIntensity;
                spotLight.color = settings.flashlightColor;
                spotLight.innerSpotAngle = settings.flashlightInnerSpotAngle;
            }

            _isOn.OnValueChanged += OnFlashlightChanged;
            spotLight.enabled = _isOn.Value;

            if (IsOwner)
            {
                var input = GetComponent<PlayerInputRouter>();
                if (input != null)
                    input.FlashlightToggled += Toggle;
            }
        }

        public override void OnNetworkDespawn()
        {
            _isOn.OnValueChanged -= OnFlashlightChanged;

            if (IsOwner)
            {
                var input = GetComponent<PlayerInputRouter>();
                if (input != null)
                    input.FlashlightToggled -= Toggle;
            }
        }

        private void Toggle()
        {
            _isOn.Value = !_isOn.Value;
        }

        private void OnFlashlightChanged(bool oldVal, bool newVal)
        {
            spotLight.enabled = newVal;
        }
    }
}
