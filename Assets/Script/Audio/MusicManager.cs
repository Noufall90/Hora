using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField]
    private MusicLibrary musicLibrary;
    [SerializeField]
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        SceneManager.sceneLoaded += OnSceneLoaded;
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (musicLibrary == null) return;

        AudioClip clip = musicLibrary.GetClipFromScene(scene.name);
        if (clip != null)
        {
            PlayMusic(clip);
        }
    }

    public void PlayMusic(string trackName, float fadeDuration = 0.5f)
    {
        if (musicLibrary == null) return;

        AudioClip clip = musicLibrary.GetClipFromName(trackName) ?? musicLibrary.GetClipFromScene(trackName);
        if (clip != null)
        {
            PlayMusic(clip, fadeDuration);
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 0.5f)
    {
        if (clip == null || musicSource == null) return;

        // Jangan restart jika clip yang sama sedang diputar
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        float startVolume = musicSource.volume > 0 ? musicSource.volume : 1f;

        while (percent < 1)
        {
            percent += Time.deltaTime * (1f / fadeDuration);
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime * (1f / fadeDuration);
            musicSource.volume = Mathf.Lerp(0, startVolume, percent);
            yield return null;
        }
    }
}