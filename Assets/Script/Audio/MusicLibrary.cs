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

    public AudioClip GetClipFromName(string trackName)
    {
        if (tracks == null) return null;

        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.trackName) && track.trackName == trackName)
            {
                return track.clip;
            }
            if (!string.IsNullOrEmpty(track.sceneName) && track.sceneName == trackName)
            {
                return track.clip;
            }
        }
        return null;
    }

    public AudioClip GetClipFromSceneName(string sceneName)
    {
        if (tracks == null) return null;

        foreach (var track in tracks)
        {
            if (!string.IsNullOrEmpty(track.sceneName) && track.sceneName == sceneName)
            {
                return track.clip;
            }
            if (!string.IsNullOrEmpty(track.trackName) && track.trackName == sceneName)
            {
                return track.clip;
            }
        }
        return null;
    }
}
 