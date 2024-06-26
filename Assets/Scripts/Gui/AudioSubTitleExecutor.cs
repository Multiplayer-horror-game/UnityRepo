using System;
using System.Collections;
using Gui.SubTitles;
using TMPro;
using UnityEngine;

namespace Gui
{
    public class AudioSubTitleExecutor : MonoBehaviour
    {
        private AudioSource _audioSource;
        
        public TextMeshProUGUI subTitlesText;
        
        private static AudioSubTitleExecutor _instance;
        
        private bool _isPlaying = false;

        public AudioSubTitleExecutor()
        {
            _instance = this;
        }
        
        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();
        }
        
        public void ExecuteSubTitles(SubTitle subtitle, Action onComplete)
        {
            if(subtitle != null)
            {
                Debug.Log("Playing audio");
                if (subtitle.audioClip != null)
                {
                    _audioSource.clip = subtitle.audioClip;
                    _audioSource.Play();
                }
                
                _isPlaying = true;
                
                StartCoroutine(playAudio(subtitle, OnCoroutineComplete, onComplete));
            }
            
        }

        IEnumerator playAudio(SubTitle subtitle, Action onComplete, Action returnToCaller)
        {
            foreach (var fragment in subtitle.subTitles)
            {
                yield return PlayFragment(fragment);
            }
            
            onComplete?.Invoke();
            returnToCaller?.Invoke();
        }
        
        IEnumerator PlayFragment(SubTitleFragments fragment)
        {
            subTitlesText.text = fragment.text;
            Debug.Log(fragment.text);
            yield return new WaitForSeconds(fragment.time);
        }
        
        public static AudioSubTitleExecutor GetInstance()
        {
            return _instance;
        }
        
        private void OnCoroutineComplete()
        {
            Debug.Log("Coroutine is complete!");
            _isPlaying = false;
        }
        
        public bool IsPlaying()
        {
            return _isPlaying;
        }
    }
}
