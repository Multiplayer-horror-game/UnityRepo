using Eflatun.SceneReference;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kart
{
    public static class Loader
    {
        public static void LoadNetwork(SceneReference scene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(scene.Name, LoadSceneMode.Single);
        }
        
        public static void LoadAdditive(SceneReference scene)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(scene.Name, LoadSceneMode.Additive);
        }
        
        public static void Unload(Scene scene)
        {
            NetworkManager.Singleton.SceneManager.UnloadScene(scene);
        }
        
        public static void LoadMultiple(SceneReference[] scenes)
        {
            for(int i = 0 ; i < scenes.Length; i++)
            {
                if (i == 0)
                {
                    NetworkManager.Singleton.SceneManager.LoadScene(scenes[i].Name, LoadSceneMode.Single);
                    continue;
                }
                Debug.Log("load addative scene");
                NetworkManager.Singleton.SceneManager.LoadScene(scenes[i].Name, LoadSceneMode.Additive);
            }
        }
        
        public static void SetAllPlayerToScene(SceneReference scene)
        {
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if(networkObject.gameObject.TryGetComponent<CharacterMovement>(out var player))
                {
                    player.gameObject.transform.SetParent(null);
                    SceneManager.MoveGameObjectToScene(player.gameObject, SceneManager.GetSceneByName(scene.Name));
                    
                    player.LeaveCar();
                }
            }
        }
    }
}