using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// IMGUI overlay showing node activation reasons. F4 toggles visibility.
    /// Reads from NodeStreamingController's last-computed state via SetActivationState().
    /// </summary>
    public class SpatialVisibilityDebugOverlay : MonoBehaviour
    {
        private bool _visible;
        private Dictionary<string, NodeActivationReason> _activationState = new();
        private GUIStyle _labelStyle;
        private bool _stylesInitialized;

        public void SetActivationState(IReadOnlyDictionary<string, NodeActivationReason> state)
        {
            _activationState.Clear();
            if (state == null) return;
            foreach (var kvp in state)
                _activationState[kvp.Key] = kvp.Value;
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F4)
            {
                _visible = !_visible;
                Event.current.Use();
            }

            if (!_visible) return;

            InitStyles();

            float x = 10f;
            float y = 200f; // Below S1A overlay (F3)

            GUI.Label(new Rect(x, y, 300f, 20f), "[F4] Node Visibility", _labelStyle);
            y += 22f;

            if (_activationState.Count == 0)
            {
                GUI.Label(new Rect(x, y, 300f, 20f), "  (no activation data)", _labelStyle);
                return;
            }

            foreach (var kvp in _activationState)
            {
                string reasons = kvp.Value == NodeActivationReason.None
                    ? "Inactive"
                    : kvp.Value.ToString();
                GUI.Label(new Rect(x, y, 400f, 20f), $"  {kvp.Key}: {reasons}", _labelStyle);
                y += 18f;
            }
        }

        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 13,
                normal = { textColor = new Color(0.9f, 0.95f, 1f) }
            };
            _stylesInitialized = true;
        }
    }
}
