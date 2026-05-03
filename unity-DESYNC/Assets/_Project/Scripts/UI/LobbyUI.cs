using Desync.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Desync.UI
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private GameBootstrap bootstrap;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private TMP_InputField ipField;
        [SerializeField] private TMP_Text statusText;

        private void Start()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
            bootstrap.OnHostStartFailed += OnHostFailed;
            bootstrap.OnConnectionFailed += OnConnectionFailed;
            ipField.text = "127.0.0.1";
            statusText.text = "Ready";
        }

        private void OnDestroy()
        {
            if (bootstrap == null) return;
            bootstrap.OnHostStartFailed -= OnHostFailed;
            bootstrap.OnConnectionFailed -= OnConnectionFailed;
        }

        private void ResetButtons()
        {
            hostButton.interactable = true;
            joinButton.interactable = true;
        }

        private void OnHostFailed(string reason)
        {
            statusText.text = reason;
            ResetButtons();
        }

        private void OnConnectionFailed(string reason)
        {
            statusText.text = reason;
            ResetButtons();
        }

        private void OnHostClicked()
        {
            statusText.text = "Starting host...";
            hostButton.interactable = false;
            joinButton.interactable = false;
            bootstrap.StartHost();
        }

        private void OnJoinClicked()
        {
            string ip = ipField.text;
            statusText.text = $"Connecting to {ip}...";
            hostButton.interactable = false;
            joinButton.interactable = false;
            bootstrap.StartClient(ip);
        }
    }
}
