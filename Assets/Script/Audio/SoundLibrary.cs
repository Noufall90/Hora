using UnityEngine;
 
[System.Serializable]
public struct SoundEffect
{
    public string[] groupIDs;
    public AudioClip[] clips;
}
 
public class SoundLibrary : MonoBehaviour
{
    public SoundEffect[] soundEffects;
 
    public AudioClip GetClipFromName(string name)
    {
        if (soundEffects == null) return null;
 
        foreach (var soundEffect in soundEffects)
        {
            if (soundEffect.groupIDs == null) continue;
 
            foreach (var id in soundEffect.groupIDs)
            {
                if (id == name)
                {
                    if (soundEffect.clips != null && soundEffect.clips.Length > 0)
                    {
                        return soundEffect.clips[Random.Range(0, soundEffect.clips.Length)];
                    }
                }
            }
        }
        return null;
    }
}