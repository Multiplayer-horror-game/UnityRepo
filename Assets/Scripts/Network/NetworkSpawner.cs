using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkSpawner : MonoBehaviour
    {
        public void Start()
        {
            int childCount = transform.childCount;

            if (childCount == 0) return;
            
            for (int i = 0; i < childCount; i++)
            {
                GameObject child = transform.GetChild(0).gameObject;
                NetworkObject networkObject = child.GetComponent<NetworkObject>();
                if (networkObject != null)
                {
                    networkObject.Spawn();
                }
            }
            
        }
    }
}
