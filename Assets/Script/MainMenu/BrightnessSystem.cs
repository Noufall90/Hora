using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSystem : MonoBehaviour
{
    private const string BrightnessKey = "BrightnessValue";

    [Header("Volume Reference")]
    public Volume sceneVolume;

    [Header("UI Reference (Optional)")]
    public Slider brightnessSlider;

    [Header("Brightness Range")]
    [SerializeField] private float minExposure = -1f;
    [SerializeField] private float maxExposure = 1f;

    [Header("Default Settings")]
    [SerializeField] private float defaultBrightness = 0.5f;

    private ColorAdjustments activeColorAdjustments;
    private Volume currentActiveVolume;
    private float baselineExposure;

    public float CurrentBrightness { get; private set; }

    private void Awake()
    {
        LoadBrightness();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        InitSliderListener();
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        RemoveSliderListener();
    }

    private void Start()
    {
        if (sceneVolume != null && sceneVolume.profile != null)
        {
            RegisterVolume(sceneVolume);
        }
        else
        {
            AutoFindAndRegisterVolume();
        }

        InitSliderListener();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (sceneVolume != null && sceneVolume.profile != null)
        {
            RegisterVolume(sceneVolume);
        }
        else
        {
            AutoFindAndRegisterVolume();
        }

        InitSliderListener();
    }

    private void InitSliderListener()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.SetValueWithoutNotify(CurrentBrightness);
            brightnessSlider.onValueChanged.RemoveListener(SetBrightnessSlider);
            brightnessSlider.onValueChanged.AddListener(SetBrightnessSlider);
        }
    }

    private void RemoveSliderListener()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(SetBrightnessSlider);
        }
    }

    public void AutoFindAndRegisterVolume()
    {
        if (currentActiveVolume != null && currentActiveVolume.profile != null)
        {
            ApplyBrightness(CurrentBrightness);
            return;
        }

        Volume[] volumes = FindObjectsByType<Volume>(FindObjectsSortMode.None);
        foreach (Volume v in volumes)
        {
            if (v != null && v.isGlobal && v.profile != null)
            {
                RegisterVolume(v);
                return;
            }
        }

        if (volumes.Length > 0 && volumes[0] != null && volumes[0].profile != null)
        {
            RegisterVolume(volumes[0]);
        }
    }

    public void RegisterVolume(Volume targetVolume)
    {
        if (targetVolume == null || targetVolume.profile == null)
        {
            return;
        }

        currentActiveVolume = targetVolume;

        VolumeProfile runtimeProfile = Instantiate(targetVolume.profile);
        targetVolume.profile = runtimeProfile;

        if (!runtimeProfile.TryGet(out activeColorAdjustments))
        {
            activeColorAdjustments = runtimeProfile.Add<ColorAdjustments>(true);
        }

        baselineExposure = activeColorAdjustments.postExposure.value;
        activeColorAdjustments.postExposure.overrideState = true;
        ApplyBrightness(CurrentBrightness);

        Debug.Log($"[BrightnessSystem] Registered volume: {targetVolume.name}, Current Brightness: {CurrentBrightness}");
    }

    public void UnregisterVolume(Volume targetVolume)
    {
        if (currentActiveVolume == targetVolume)
        {
            currentActiveVolume = null;
            activeColorAdjustments = null;
        }
    }

    public void SetBrightnessSlider(float value)
    {
        CurrentBrightness = value;
        SaveBrightness();

        if (brightnessSlider != null && !Mathf.Approximately(brightnessSlider.value, value))
        {
            brightnessSlider.SetValueWithoutNotify(value);
        }

        if (activeColorAdjustments == null)
        {
            if (sceneVolume != null && sceneVolume.profile != null)
            {
                RegisterVolume(sceneVolume);
            }
            else
            {
                AutoFindAndRegisterVolume();
            }
        }

        ApplyBrightness(value);
    }

    public void SaveBrightness()
    {
        PlayerPrefs.SetFloat(BrightnessKey, CurrentBrightness);
        PlayerPrefs.Save();
    }

    public void LoadBrightness()
    {
        CurrentBrightness = PlayerPrefs.GetFloat(BrightnessKey, defaultBrightness);

        if (brightnessSlider != null)
        {
            brightnessSlider.SetValueWithoutNotify(CurrentBrightness);
        }

        ApplyBrightness(CurrentBrightness);
    }

    public void RestoreDefault()
    {
        SetBrightnessSlider(defaultBrightness);
    }

    private void ApplyBrightness(float value)
    {
        if (activeColorAdjustments == null)
        {
            if (sceneVolume != null && sceneVolume.profile != null)
            {
                RegisterVolume(sceneVolume);
            }
            else
            {
                AutoFindAndRegisterVolume();
            }
        }

        if (activeColorAdjustments == null)
        {
            Debug.LogWarning("[BrightnessSystem] Cannot apply brightness - no active Global Volume with ColorAdjustments found in scene!");
            return;
        }

        float exposureValue = baselineExposure + Mathf.Lerp(minExposure, maxExposure, value);
        activeColorAdjustments.postExposure.overrideState = true;
        activeColorAdjustments.postExposure.value = exposureValue;
    }
}