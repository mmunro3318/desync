using System.Collections.Generic;
using UnityEngine;
using Desync.World.Graph.Runtime;

namespace Desync.World.Graph.Debug
{
    /// <summary>
    /// IMGUI overlay showing node activation reasons. F4 toggles visibility.
    /// Lazily finds NodeStreamingController and polls its LastResult each frame.
    /// </summary>
    public class SpatialVisibilityDebugOverlay : MonoBehaviour
    {
        private bool _visible;
        private Dictionary<string, NodeActivationReason> _activationState = new();
        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;
        private bool _stylesInitialized;
        private NodeStreamingController _controller;
        private bool _controllerSearched;

        public void SetActivationState(IReadOnlyDictionary<string, NodeActivationReason> state)
        {
            _activationState.Clear();
            if (state == null) return;
            foreach (var kvp in state)
                _activationState[kvp.Key] = kvp.Value;
        }

        /// <summary>
        /// Finds the scene's NodeStreamingController (lazy) and syncs activation state.
        /// Public so tests can invoke without requiring Update() lifecycle.
        /// </summary>
        public void PollController()
        {
            if (_controller == null && !_controllerSearched)
            {
                _controller = FindAnyObjectByType<NodeStreamingController>();
                _controllerSearched = true;
            }

            if (_controller == null) return;

            SetActivationState(_controller.LastResult);
        }

        private void Update()
        {
            PollController();
        }

        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.F4)
            {
                _visible = !_visible;
                _controllerSearched = false; // Re-search on next toggle-on
                Event.current.Use();
            }

            if (!_visible) return;

            InitStyles();
            DrawHeader();
            DrawNodeStates();
        }

        private void DrawHeader()
        {
            float x = 10f;
            float y = 200f;
            GUI.Label(new Rect(x, y, 300f, 20f), "[F4] Node Visibility", _headerStyle);
        }

        private void DrawNodeStates()
        {
            float x = 10f;
            float y = 222f;

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
            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.7f, 1f, 0.7f) }
            };
            _stylesInitialized = true;
        }
    }
}
