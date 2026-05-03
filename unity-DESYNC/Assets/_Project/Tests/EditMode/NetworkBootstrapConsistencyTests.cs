using NUnit.Framework;
using Unity.Netcode;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Desync.Tests.EditMode
{
    /// <summary>
    /// Regression test for TD0002 — guards against the NGO drift pattern where
    /// <see cref="NetworkManager.ConnectionApprovalCallback"/> is wired up but
    /// <see cref="NetworkConfig.ConnectionApproval"/> is left disabled.
    ///
    /// Behavioral, not source-string coupled (per F-cleanup plan decision D7):
    /// loads Bootstrap.unity and inspects the scene-serialized state on
    /// NetworkManager.
    /// </summary>
    public class NetworkBootstrapConsistencyTests
    {
        private const string BootstrapScenePath =
            "Assets/_Project/Scenes/Bootstrap.unity";

        private Scene _openedScene;
        private bool _sceneWasOpened;

        [SetUp]
        public void OpenBootstrapScene()
        {
            _openedScene = EditorSceneManager.OpenScene(
                BootstrapScenePath, OpenSceneMode.Single);
            _sceneWasOpened = _openedScene.IsValid() && _openedScene.isLoaded;
            Assert.IsTrue(_sceneWasOpened,
                $"Failed to open scene at '{BootstrapScenePath}'.");
        }

        [Test]
        public void ConnectionApproval_FlagAndCallback_AreConsistent()
        {
            var networkManager = Object.FindAnyObjectByType<NetworkManager>();
            Assert.IsNotNull(networkManager,
                $"No NetworkManager found in '{BootstrapScenePath}'.");

            bool flag = networkManager.NetworkConfig != null
                && networkManager.NetworkConfig.ConnectionApproval;
            bool callbackWired = networkManager.ConnectionApprovalCallback != null;

            // The drift TD0002 caught: callback assigned but flag disabled.
            // NGO logs a warning at runtime when this happens.
            if (callbackWired)
            {
                Assert.IsTrue(flag,
                    "ConnectionApprovalCallback assigned but " +
                    "NetworkConfig.ConnectionApproval is false in " +
                    "Bootstrap.unity — see TD0002.");
            }
        }

        [Test]
        public void Bootstrap_PostFix_HasConnectionApprovalDisabled()
        {
            // Locks in the post-fix state: with the callback removed from
            // GameBootstrap.cs, the scene-serialized flag must remain false.
            // If a future PR enables the flag without deciding on lobby auth,
            // this test fails and forces the conversation.
            var networkManager = Object.FindAnyObjectByType<NetworkManager>();
            Assert.IsNotNull(networkManager,
                $"No NetworkManager found in '{BootstrapScenePath}'.");
            Assert.IsNotNull(networkManager.NetworkConfig,
                "NetworkManager.NetworkConfig is null.");

            Assert.IsFalse(networkManager.NetworkConfig.ConnectionApproval,
                "NetworkConfig.ConnectionApproval should be false in M1 " +
                "(no lobby auth scope yet — see TD0002).");
        }
    }
}
