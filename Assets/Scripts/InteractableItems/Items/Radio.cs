using Gui;
using Gui.SubTitles;
using UnityEngine;

namespace InteractableItems.Items
{
    public class Radio : MonoBehaviour, IInteractable
    {
        private AudioSubTitleExecutor _audioSubTitleExecutor;
        private AudioSource _audioSource;
        
        public SubTitle subtitle;
        
        private void Start()
        {
            _audioSubTitleExecutor = AudioSubTitleExecutor.GetInstance();
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void Interact(Transform transform)
        {
            _audioSubTitleExecutor.ExecuteSubTitles(subtitle);
            _audioSource.Play();
        }
    }
}