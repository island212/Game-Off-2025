using System;
using UnityEngine;
using UnityEngine.UI;

public class OptionPanel : MonoBehaviour
{
    [SerializeField] private Slider MasterVolumeSlider;
    [SerializeField] private Slider SoundFXVolumeSlider;
    [SerializeField] private Slider SoundBGMVolumeSlider;

    private void OnEnable()
    {
        Console.WriteLine("Option Panel Enabled");
        MasterVolumeSlider.value = SoundFXManager.Instance.GetMasterVolume();
        SoundFXVolumeSlider.value = SoundFXManager.Instance.GetSoundFXVolume();
        SoundBGMVolumeSlider.value = SoundFXManager.Instance.GetSoundBGMVolume();

        MasterVolumeSlider.onValueChanged.AddListener(delegate { SoundFXManager.Instance.SetMasterVolume(MasterVolumeSlider.value); });
        SoundFXVolumeSlider.onValueChanged.AddListener(delegate { SoundFXManager.Instance.SetSoundFXVolume(SoundFXVolumeSlider.value); });
        SoundBGMVolumeSlider.onValueChanged.AddListener(delegate { SoundFXManager.Instance.SetSoundBGMVolume(SoundBGMVolumeSlider.value); });
    }
}
