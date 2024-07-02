using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkSpawnConnector : MonoBehaviour
    {
        private static NetworkSpawnConnector _instance;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
        }
        
        public static NetworkSpawnConnector GetInstance()
        {
            return _instance;
        }
        
        public void SpawnNetworkObject(GameObject _gameObject, Transform _transform)
        {
            if (!NetworkManager.Singleton.IsHost) return;
            
            NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(_gameObject.GetComponent<NetworkObject>(), 0, true, false, false, _transform.position, _transform.rotation);
        }
    }
}
