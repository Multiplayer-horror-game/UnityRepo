using Eflatun.SceneReference;
using UnityEngine;
using UnityEngine.UI;

namespace Kart
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] Button createLobbyButton;
        [SerializeField] Button joinLobbyButton;
        [SerializeField] SceneReference gameScene;
        [SerializeField] private SceneReference persistantData;

        void Awake()
        {
            createLobbyButton.onClick.AddListener(CreateGame);
            joinLobbyButton.onClick.AddListener(JoinGame);
        }

        async void CreateGame()
        {
            Debug.Log("Creating Game");
            await Multiplayer.Instance.CreateLobby();
            Loader.LoadNetwork(gameScene);
            Loader.LoadAdditive(persistantData);
        }

        async void JoinGame()
        {
            Debug.Log("Joining Game");
            await Multiplayer.Instance.QuickJoinLobby();
        }
    }
}