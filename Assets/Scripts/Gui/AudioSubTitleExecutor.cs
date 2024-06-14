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
        
        public SubTitle temp_subtitle;

        public AudioSubTitleExecutor()
        {
            _instance = this;
        }
        
        public void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            
            ExecuteSubTitles(temp_subtitle);
        }
        
        public void ExecuteSubTitles(SubTitle subtitle)
        {
            Debug.Log("Playing audio");
            _audioSource.clip = subtitle.audioClip;
            _audioSource.Play();

            StartCoroutine(playAudio(subtitle));
        }

        IEnumerator playAudio(SubTitle subtitle)
        {
            foreach (var fragment in subtitle.subTitles)
            {
                yield return PlayFragment(fragment);
            }
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
    }
}
