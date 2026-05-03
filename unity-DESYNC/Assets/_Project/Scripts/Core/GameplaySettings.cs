using UnityEngine;

namespace Desync.Core
{
    [CreateAssetMenu(fileName = "GameplaySettings", menuName = "Desync/Gameplay Settings")]
    public class GameplaySettings : ScriptableObject
    {
        [Header("Player Body")]
        [Tooltip("CharacterController capsule radius. Capsule diameter must be < doorwayWidth - 2*skinWidth for clean passage. Default 0.30 → 0.6m capsule, 0.20m clearance per side through a 1.0m door.")]
        public float playerRadius = 0.30f;
        public float playerHeight = 2.0f;
        public float playerStepOffset = 0.5f;
        public float playerSlopeLimit = 45f;
        public float playerSkinWidth = 0.08f;
        [Tooltip("Reference value: target doorway opening width in House_Graybox (meters). Used to validate playerRadius. If you change this, audit door geometry in the scene.")]
        public float doorwayWidth = 1.0f;

        [Header("Player Movement")]
        public float walkSpeed = 3.5f;
        public float sprintSpeed = 5.5f;
        public float gravity = -9.81f;

        [Header("Player Look")]
        public float mouseSensitivity = 0.15f;
        public float pitchClamp = 80f;

        [Header("Flashlight")]
        public float flashlightRange = 15f;
        public float flashlightSpotAngle = 45f;
        public float flashlightIntensity = 3f;
        public Color flashlightColor = Color.white;
        public float flashlightInnerSpotAngle = 21f;

        [Header("Ambient Audio")]
        public float ambientDroneVolume = 0.15f;
        [Tooltip("Min seconds between random one-shot sounds")]
        public float ambientOneShotMinInterval = 15f;
        [Tooltip("Max seconds between random one-shot sounds")]
        public float ambientOneShotMaxInterval = 45f;
        public float ambientOneShotVolume = 0.3f;
        [Tooltip("Max distance from player to spawn one-shot sounds")]
        public float ambientOneShotRange = 15f;

        [Header("Footstep Audio")]
        public float footstepWalkInterval = 0.5f;
        public float footstepSprintInterval = 0.33f;
        public float footstepVolume = 0.4f;
    }
}
