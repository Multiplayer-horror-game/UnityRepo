using System;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkSpawner : MonoBehaviour
    {
        private static NetworkSpawnConnector _instance;
        
        public DoorPrefab[] prefabs;
        public void Start()
        {
            if (_instance == null)
            {
                _instance = NetworkSpawnConnector.GetInstance();
            }
            
            if (!NetworkManager.Singleton.IsHost) return;

            foreach (var obj in prefabs)
            {
                Debug.Log("Spawning " + obj.prefab.name);
                
                _instance.SpawnNetworkObject(obj.prefab, obj.transform);
            }
        }
    }
    
    [Serializable]
    public struct DoorPrefab
    {
        public GameObject prefab;
        public Transform transform;
    }
}
