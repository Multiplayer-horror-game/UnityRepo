using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kart
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] Button createLobbyButton;
        [SerializeField] Button joinLobbyButton;
        [SerializeField] SceneReference gameScene;

        void Awake()
        {
            createLobbyButton.onClick.AddListener(CreateGame);
            joinLobbyButton.onClick.AddListener(JoinGame);
        }

        async void CreateGame()
        {
            Debug.Log("Creating Game");
            await Multiplayer.Instance.CreateLobby();
            Scene scene = SceneManager.GetSceneByName("Lobby");
            
            Loader.LoadAdditive(gameScene);
            
            SceneManager.UnloadSceneAsync(scene);
        }

        async void JoinGame()
        {
            Debug.Log("Joining Game");
            await Multiplayer.Instance.QuickJoinLobby();
        }
    }
}