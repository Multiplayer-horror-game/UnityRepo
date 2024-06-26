using Eflatun.SceneReference;
using Gui;
using Gui.SubTitles;
using Kart;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InteractableItems.Items
{
    public class Radio : NetworkBehaviour, IInteractable
    {
        private AudioSubTitleExecutor _audioSubTitleExecutor;
        private AudioSource _audioSource;
        
        public SubTitle subtitle;
        public SceneReference roomgen;
        public SceneReference persistantData;
        
        private void Start()
        {
            _audioSubTitleExecutor = AudioSubTitleExecutor.GetInstance();
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void Interact(Transform transform)
        {
            
            if (IsHost)
            {
                _audioSubTitleExecutor.ExecuteSubTitles(subtitle, AudioDone);
                _audioSource.Play();
                InteractClientRpc();
            }
            else
            {
                _audioSubTitleExecutor.ExecuteSubTitles(subtitle, AudioDone);
                _audioSource.Play();
                InteractServerRpc();
            }
        }

        [ClientRpc]
        private void InteractClientRpc()
        {
            if (!IsHost)
            {
                _audioSubTitleExecutor.ExecuteSubTitles(subtitle, null);
                _audioSource.Play();
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc()
        {
            _audioSubTitleExecutor.ExecuteSubTitles(subtitle, AudioDone);
            _audioSource.Play();
            InteractClientRpc();
        }

        private void AudioDone()
        {
            Loader.SetAllPlayerToScene(persistantData);
            
            if (IsHost)
            {
                //set all players to persistant data
                LoadAdditiveServerRpc();
            }

            SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName("Fase1"));
        }

        [ServerRpc(RequireOwnership = false)]
        private void LoadAdditiveServerRpc()
        {
            Loader.LoadAdditive(roomgen);
            Scene currentScene = SceneManager.GetSceneByName("Fase1");
            SceneManager.UnloadSceneAsync(currentScene);

            LoadAdditiveClientRpc();
        }

        [ClientRpc]
        private void LoadAdditiveClientRpc()
        {
            if (!IsServer)
            {
                Loader.LoadAdditive(roomgen);
                Scene currentScene = SceneManager.GetSceneByName("Fase1");
                SceneManager.UnloadSceneAsync(currentScene);
            }
        }
    }
}