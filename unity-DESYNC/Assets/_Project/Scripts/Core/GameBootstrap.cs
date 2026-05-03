using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Desync.Core
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private string gameplaySceneName = "House_Graybox";

        private static GameBootstrap _instance;

        public event System.Action<string> OnHostStartFailed;

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
            if (!NetworkManager.Singleton.StartHost())
            {
                Debug.LogError("[GameBootstrap] StartHost failed.");
                OnHostStartFailed?.Invoke("Failed to start host.");
                return;
            }

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
            transport.SetConnectionData(ipAddress, 7777);
            NetworkManager.Singleton.StartClient();
        }
    }
}
