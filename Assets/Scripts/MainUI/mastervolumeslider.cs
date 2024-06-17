using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Volumesettings : MonoBehaviour
{
  [SerializeField] private AudioMixer myMixer;
  [SerializeField] private Slider  musicSlider;

  public void SetMusicVolume()
  {
    float volume = musicSlider.value;
  }

}
