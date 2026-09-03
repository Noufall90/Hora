using UnityEngine;

[System.Serializable]
public struct MusicTrack
{
    public string[] sceneNames;
    public AudioClip clip;
}

public class MusicLibrary : MonoBehaviour
{
    public MusicTrack[] tracks;

    public AudioClip GetClipFromName(string name)
    {
        return GetClipFromSceneName(name);
    }

    public AudioClip GetClipFromSceneName(string sceneName)
    {
        if (tracks == null) return null;

        foreach (var track in tracks)
        {
            if (track.sceneNames == null) continue;

            foreach (var name in track.sceneNames)
            {
                if (!string.IsNullOrEmpty(name) && name == sceneName)
                {
                    return track.clip;
                }
            }
        }
        return null;
    }
}