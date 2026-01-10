using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    private AudioSource musicSource;
    private AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    [Header("Sound Effects")]
    public AudioClip sliceSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip buttonClickSound;
    public AudioClip perfectCutSound;
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetupAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SetupAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
    }

    void Start()
    {
        if (backgroundMusic != null)
        {
            PlayMusic(backgroundMusic);
        }
    }

    public void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    public void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    public void PlaySliceSound()
    {
        PlaySFX(sliceSound);
    }

    public void PlaySuccessSound()
    {
        PlaySFX(successSound);
    }

    public void PlayFailSound()
    {
        PlaySFX(failSound);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound);
    }

    public void PlayPerfectCut()
    {
        PlaySFX(perfectCutSound);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void ToggleMusic()
    {
        if (musicSource != null)
        {
            if (musicSource.isPlaying)
            {
                musicSource.Pause();
            }
            else
            {
                musicSource.UnPause();
            }
        }
    }

    public void ToggleSFX()
    {
        sfxVolume = sfxVolume > 0 ? 0 : 1;
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}