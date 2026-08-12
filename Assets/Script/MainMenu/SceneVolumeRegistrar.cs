using UnityEngine;
using UnityEngine.Rendering;

public class SceneVolumeRegistrar : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Volume in this scene that should be controlled by the global brightness setting.")]
    [SerializeField] private Volume sceneVolume;

    private void Start()
    {
        if (BrightnessSystem.Instance != null)
        {
            BrightnessSystem.Instance.RegisterVolume(sceneVolume);
        }
    }

    private void OnDestroy()
    {
        if (BrightnessSystem.Instance != null)
        {
            BrightnessSystem.Instance.UnregisterVolume(sceneVolume);
        }
    }
}