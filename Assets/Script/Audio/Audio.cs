using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Audio : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    private void Start()
    {
        LoadVolume();
        string currentSceneName = SceneManager.GetActiveScene().name;
        MusicManager.Instance.PlayMusic(currentSceneName);
    }

    public void PlaySceneMusic()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        MusicManager.Instance.PlayMusic(currentSceneName);
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }

    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }

    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);

        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }

    public void LoadVolume()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
            musicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        if (PlayerPrefs.HasKey("SFXVolume"))
            sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume");

        if (musicSlider != null)
            UpdateMusicVolume(musicSlider.value);
        if (sfxSlider != null)
            UpdateSoundVolume(sfxSlider.value);
    }

    public void RestoreDefault()
    {
        float defaultVolume = 0f;

        if (musicSlider != null)
            musicSlider.value = defaultVolume;

        if (sfxSlider != null)
            sfxSlider.value = defaultVolume;

        UpdateMusicVolume(defaultVolume);
        UpdateSoundVolume(defaultVolume);

        PlayerPrefs.SetFloat("MusicVolume", defaultVolume);
        PlayerPrefs.SetFloat("SFXVolume", defaultVolume);
        PlayerPrefs.Save();
    }
}
