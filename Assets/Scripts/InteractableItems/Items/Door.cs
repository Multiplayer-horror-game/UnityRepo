using Unity.Netcode;
using UnityEngine;

namespace InteractableItems.Items
{
    public class Door : NetworkBehaviour, IInteractable
    {
        private Animator _animator;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }
        
        public void Interact(Transform transform)
        {
            if (IsHost)
            {
                _animator.SetTrigger("DoorState");
                InteractClientRpc();
            }
            else
            {
                InteractServerRpc();
            }
        }
        
        [ClientRpc]
        private void InteractClientRpc()
        {
            if (!IsHost)
            {
                _animator.SetTrigger("DoorState");
            }
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc()
        {
            _animator.SetTrigger("DoorState");
            InteractClientRpc();
        }
        
        
    }
}