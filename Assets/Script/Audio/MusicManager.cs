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

    private Coroutine crossfadeCoroutine;
 
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;

            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
        }

        if (musicLibrary == null)
        {
            musicLibrary = GetComponent<MusicLibrary>();
        }

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
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
        PlayMusic(scene.name);
    }
 
    public void PlayMusic(string trackOrSceneName, float fadeDuration = 0.5f)
    {
        if (musicLibrary == null)
        {
            musicLibrary = GetComponent<MusicLibrary>();
            if (musicLibrary == null)
            {
                return;
            }
        }

        AudioClip clip = musicLibrary.GetClipFromSceneName(trackOrSceneName);
        if (clip == null)
        {
            clip = musicLibrary.GetClipFromName(trackOrSceneName);
        }

        if (clip == null)
        {
            Debug.LogWarning($"[MusicManager] Musik/Audio untuk scene atau track '{trackOrSceneName}' tidak ditemukan di MusicLibrary!");
            return;
        }

        if (musicSource == null)
        {
            musicSource = GetComponent<AudioSource>();
            if (musicSource == null)
            {
                return;
            }
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        if (crossfadeCoroutine != null)
        {
            StopCoroutine(crossfadeCoroutine);
        }
        crossfadeCoroutine = StartCoroutine(AnimateMusicCrossfade(clip, fadeDuration));
    }
 
    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float fadeDuration = 0.5f)
    {
        float percent = 0;
        float startVolume = musicSource.volume;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }
 
        musicSource.clip = nextTrack;
        musicSource.Play();
 
        percent = 0;
        while (percent < 1)
        {
            percent += Time.deltaTime / fadeDuration;
            musicSource.volume = Mathf.Lerp(0, 1f, percent);
            yield return null;
        }

        crossfadeCoroutine = null;
    }
}