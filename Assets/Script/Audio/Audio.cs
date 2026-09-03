using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Audio : MonoBehaviour
{
    [Header("Audio Mixer Reference")]
    public AudioMixer audioMixer;

    [Header("UI Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Default Volume Settings")]
    [SerializeField] private float defaultMusicVolume = 0f;
    [SerializeField] private float defaultSfxVolume = 0f;

    private void Start()
    {
        LoadVolume();

        if (MusicManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            MusicManager.Instance.PlayMusic(currentSceneName);
        }
    }

    public void PlaySceneMusic()
    {
        if (MusicManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            MusicManager.Instance.PlayMusic(currentSceneName);
        }
    }

    public void UpdateMusicVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("MusicVolume", volume);
        }
    }

    public void UpdateSoundVolume(float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat("SFXVolume", volume);
        }
    }

    public void SaveVolume()
    {
        if (audioMixer == null) return;

        if (audioMixer.GetFloat("MusicVolume", out float musicVolume))
        {
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
        }

        if (audioMixer.GetFloat("SFXVolume", out float sfxVolume))
        {
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }

        PlayerPrefs.Save();
    }

    public void LoadVolume()
    {
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolume", defaultSfxVolume);

        if (musicSlider != null)
        {
            musicSlider.value = musicVol;
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = sfxVol;
        }

        UpdateMusicVolume(musicVol);
        UpdateSoundVolume(sfxVol);
    }

    public void RestoreDefault()
    {
        if (musicSlider != null)
        {
            musicSlider.value = defaultMusicVolume;
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = defaultSfxVolume;
        }

        UpdateMusicVolume(defaultMusicVolume);
        UpdateSoundVolume(defaultSfxVolume);

        PlayerPrefs.SetFloat("MusicVolume", defaultMusicVolume);
        PlayerPrefs.SetFloat("SFXVolume", defaultSfxVolume);
        PlayerPrefs.Save();
    }
}
