using Unity.Netcode;
using UnityEngine;

namespace InteractableItems.Items
{
    public class Objective : NetworkBehaviour, IInteractable
    {
        [SerializeField] public GameObject paper;

        private void Start()
        {

        }

        public void Interact(Transform transform)
        {
            if (IsHost)
            {
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
                paper.gameObject.SetActive(false);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void InteractServerRpc()
        {
            InteractClientRpc();
            paper.gameObject.SetActive(false);
        }


    }
}