using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkCar : NetworkBehaviour
    {
        private static NetworkCar _instance;
        public List<Seat> seats = new();
        private int index = 0;

        public NetworkCar()
        {
            _instance = this;
        }


        void Start()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }
        }

        void OnDestroy()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
        
        private void OnClientDisconnected(ulong clientId)
        {
            CharacterMovement player = FindPlayer(clientId);
            if (player != null)
            {
                player.transform.SetParent(null);
                
            }
        }

        private void OnClientConnected(ulong clientId)
        {
            CharacterMovement player = FindPlayer(clientId);
            if (player != null)
            {
                AttachPlayerToCar(player);
            }
        }

        private CharacterMovement FindPlayer(ulong clientId)
        {
            foreach (var networkObject in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                if (networkObject.TryGetComponent<CharacterMovement>(out var player) && player.OwnerClientId == clientId)
                {
                    return player;
                }
            }
            return null;
        }

        [ServerRpc(RequireOwnership = false)]
        public void AttachPlayerToCarServerRpc(ulong clientId)
        {
            CharacterMovement player = FindPlayer(clientId);
            if (player != null)
            {
                foreach (var seat in seats)
                {
                    if (seat.IsOccupied())
                    {
                        Debug.Log("Seat is occupied");
                        continue;
                    }
                    
                    AttachPlayerToCarClientRpc(clientId, seat.position, seat.rotation);
                    
                    seat.AttachPlayer();
                    
                    break;
                }
                
            }
        }

        [ClientRpc]
        private void AttachPlayerToCarClientRpc(ulong clientId, Vector3 position, Quaternion rotation)
        {
            CharacterMovement player = FindPlayer(clientId);
            if (player != null)
            {
                AttachPlayerToCar(player, position, rotation);
            }
        }

        private void AttachPlayerToCar(CharacterMovement player, Vector3 position, Quaternion rotation)
        {
            player.transform.SetParent(transform);
            
            StartCoroutine(WaitAndReposition(player, position, rotation));
            
        }

        private IEnumerator WaitAndReposition(CharacterMovement player, Vector3 position, Quaternion rotation)
        {
            yield return new WaitForSeconds(1f);
            
            player.transform.localPosition = position;
            player.transform.localRotation = rotation;
        }

        private void AttachPlayerToCar(CharacterMovement player)
        {
            foreach (var seat in seats)
            {
                if (seat.IsOccupied())
                {
                    Debug.Log("Seat is occupied");
                    continue;
                }
                    
                AttachPlayerToCar(player, seat.position, seat.rotation);
                    
                seat.AttachPlayer();
                    
                break;
            }
        }
        
        public static NetworkCar Instance => _instance;
    }
}