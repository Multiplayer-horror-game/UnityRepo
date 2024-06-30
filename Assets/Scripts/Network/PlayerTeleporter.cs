using System.Collections;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class PlayerTeleporter : NetworkBehaviour
    {
    
        public override void OnNetworkSpawn()
        {
            StartCoroutine(teleportPlayer());
        }
        
        private IEnumerator teleportPlayer()
        {
            yield return new WaitForSeconds(2f);
            Debug.Log("Teleporting players");
            
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if(networkObject.gameObject.TryGetComponent<CharacterMovement>(out var player) && player.IsLocalPlayer)
                {
                    Debug.Log("Teleporting player");
                    player.transform.position = transform.position;
                    player.transform.rotation = transform.rotation;
                    
                    player.ResetCameraRotation();
                }
            }
        }
    }
}
