using System;
using UnityEngine;

namespace InteractableItems
{
    public class InteractManager : MonoBehaviour
    {
        public void Update()
        {
            throw new NotImplementedException();
        }

        public void OnInteractButtonDown()
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 5))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    interactable.Interact(transform);
                }
            }
        }
    }
}