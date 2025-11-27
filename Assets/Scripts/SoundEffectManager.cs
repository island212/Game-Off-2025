using UnityEngine;
using UnityEngine.Audio;

public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private AudioMixer audioMixer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlaySound(AudioClip audioClip, Transform spawnTransform, float volume = 1f)
    {
        AudioSource source = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        source.clip = audioClip;
        source.volume = volume;
        
        // Configure 3D spatial audio
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 0f;
        source.maxDistance = 25f;
        
        source.Play();
        float clipLength = audioClip.length;
        Destroy(source.gameObject, clipLength);
    }

    public void PlayRandomSound(AudioClip[] audioClips, Transform spawnTransform, float volume = 1f)
    {
        int rand = Random.Range(0, audioClips.Length);
        AudioSource source = Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity);
        source.clip = audioClips[rand];
        source.volume = volume;
        
        // Configure 3D spatial audio
        source.spatialBlend = 1f; 
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 1f;
        source.maxDistance = 50f;
        
        source.Play();
        float clipLength = source.clip.length;
        Destroy(source.gameObject, clipLength);
    }

    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSoundFXVolume(float volume)
    {
        audioMixer.SetFloat("SoundFXVolume", Mathf.Log10(volume) * 20);
    }

    public void SetSoundBGMVolume(float volume)
    {
        audioMixer.SetFloat("SoundBGMVolume", Mathf.Log10(volume) * 20);
    }

    public float GetMasterVolume()
    {
        audioMixer.GetFloat("MasterVolume", out float volume);
        return Mathf.Pow(10, volume / 20);
    }

    public float GetSoundFXVolume()
    {
        audioMixer.GetFloat("SoundFXVolume", out float volume);
        return Mathf.Pow(10, volume / 20);
    }

    public float GetSoundBGMVolume()
    {
        audioMixer.GetFloat("SoundBGMVolume", out float volume);
        return Mathf.Pow(10, volume / 20);
    }
}
