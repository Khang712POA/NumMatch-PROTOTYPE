using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AudioElement
{
    public string name;
    public AudioClip clip;
}

public class AudioManager : MonoBehaviour
{
    private static AudioManager instance;
    public static AudioManager Instance { get => instance; set => instance = value; }

    [Header("Audio Sources")]
    public AudioSource music;
    public AudioSource sfx;

    [Header("Audio Clips")]
    [Space(5)]
    [SerializeField] List<AudioElement> musicClips = new();
    [SerializeField] List<AudioElement> sfxClips = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void OnEnable()
    {
        music = Instantiate(new GameObject("Music").AddComponent<AudioSource>(), transform);
        sfx = Instantiate(new GameObject("SFX").AddComponent<AudioSource>(), transform);
        music.loop = true;
        sfx.playOnAwake = false;
    }

    public void PlayMusic(string clipName, float delayedTime = 0)
    {
        music.clip = musicClips.Find(x => x.name.Equals(clipName)).clip;
        music.PlayDelayed(delayedTime);
    }

    public void PlayMusic(AudioClip clip, float delayedTime = 0)
    {
        if (music.clip == clip && music.isPlaying)
            return;

        music.clip = clip;
        music.PlayDelayed(delayedTime);
    }


    public void PlaySFX(string clipName)
    {
        sfx.PlayOneShot(sfxClips.Find(x => x.name.Equals(clipName)).clip);
    }

    public void PlaySFX(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        //SettingManager.Instance.currentSettings.musicVolume = volume;
        music.volume = volume;
    }

    public void SetSFXVolume(float volume)
    {
        //SettingManager.Instance.currentSettings.sfxVolume = volume;
        sfx.volume = volume;
    }

    public void SetMusicMute(bool mute)
    {
        //SettingManager.Instance.currentSettings.musicMute = mute;
        music.gameObject.SetActive(!mute);
    }

    public void SetSFXMute(bool mute)
    {
        //SettingManager.Instance.currentSettings.sfxMute = mute;
        sfx.gameObject.SetActive(!mute);
    }
}
