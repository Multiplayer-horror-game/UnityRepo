using System;
using UnityEngine;

namespace InteractableItems
{
    public class InteractManager : MonoBehaviour
    {
        private Camera _camera;
        
        private void Start()
        {
            _camera = Camera.main;
        }
        
        
        public void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse button down");
                OnInteractButtonDown();
            }
        }

        public void OnInteractButtonDown()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
            
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
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