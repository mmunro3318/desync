using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Desync.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "House_Prototype";
        [SerializeField] private ushort port = 7777;

        private static GameBootstrap _instance;

        public event System.Action<string> OnHostStartFailed;
        public event System.Action<string> OnConnectionFailed;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            // Bind to all interfaces so remote clients can connect
            var transport = NetworkManager.Singleton
                .GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData("0.0.0.0", port, "0.0.0.0");

            Debug.Log($"[GameBootstrap] Transport configured: listen={transport.ConnectionData.ServerListenAddress}, address={transport.ConnectionData.Address}, port={transport.ConnectionData.Port}");

            RegisterCallbacks();

            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("[GameBootstrap] StartHost failed.");
                UnregisterCallbacks();
                OnHostStartFailed?.Invoke("Failed to start host.");
                return;
            }

            Debug.Log($"[GameBootstrap] Host started successfully. Listening on 0.0.0.0:{port} UDP.");

            bool sceneExists = false;
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                var path = SceneUtility.GetScenePathByBuildIndex(i);
                if (System.IO.Path.GetFileNameWithoutExtension(path) == gameplaySceneName)
                {
                    sceneExists = true;
                    break;
                }
            }

            if (!sceneExists)
            {
                Debug.LogError($"[GameBootstrap] Scene '{gameplaySceneName}' not found in build settings. Shutting down host.");
                NetworkManager.Singleton.Shutdown();
                OnHostStartFailed?.Invoke($"Scene '{gameplaySceneName}' not found.");
                return;
            }

            NetworkManager.Singleton.SceneManager.LoadScene(
                gameplaySceneName, LoadSceneMode.Single);
        }

        public void StartClient(string ipAddress)
        {
            var transport = NetworkManager.Singleton
                .GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.SetConnectionData(ipAddress, port);

            RegisterCallbacks();

            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("[GameBootstrap] StartClient failed.");
                UnregisterCallbacks();
                OnConnectionFailed?.Invoke("Failed to start client.");
            }
        }

        private void RegisterCallbacks()
        {
            var nm = NetworkManager.Singleton;
            nm.OnClientDisconnectCallback += OnClientDisconnect;
            nm.OnTransportFailure += OnTransportFailed;
        }

        private void UnregisterCallbacks()
        {
            var nm = NetworkManager.Singleton;
            if (nm == null) return;
            nm.OnClientDisconnectCallback -= OnClientDisconnect;
            nm.OnTransportFailure -= OnTransportFailed;
        }

        private void OnClientDisconnect(ulong clientId)
        {
            // Only handle our own disconnection (local client ID)
            if (NetworkManager.Singleton == null) return;
            if (clientId != NetworkManager.Singleton.LocalClientId) return;

            Debug.LogWarning("[GameBootstrap] Local client disconnected.");
            UnregisterCallbacks();
            OnConnectionFailed?.Invoke("Disconnected from host.");
        }

        private void OnTransportFailed()
        {
            Debug.LogError("[GameBootstrap] Transport failure.");
            UnregisterCallbacks();
            OnConnectionFailed?.Invoke("Network transport failed.");
        }

        private void OnDestroy()
        {
            UnregisterCallbacks();
        }
    }
}
