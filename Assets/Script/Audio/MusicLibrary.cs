using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string sceneName;
    public string trackName;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;

    public AudioClip GetClipFromScene(string sceneName)
    {
        if (tracks == null || string.IsNullOrEmpty(sceneName)) return null;

        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.sceneName) && track.sceneName == sceneName)
            {
                return track.clip;
            }
        }

        // Fallback: periksa juga jika sceneName cocok dengan trackName
        return GetClipFromName(sceneName);
    }

    public AudioClip GetClipFromName(string trackName)
    {
        if (tracks == null || string.IsNullOrEmpty(trackName)) return null;

        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.trackName) && track.trackName == trackName)
            {
                return track.clip;
            }
        }
        return null;
    }
}