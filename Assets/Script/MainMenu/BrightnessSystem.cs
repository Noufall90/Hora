using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BrightnessSystem : MonoBehaviour
{
    private static BrightnessSystem instance;
    public static BrightnessSystem Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<BrightnessSystem>();
                if (instance == null)
                {
                    GameObject go = new GameObject("BrightnessSystem");
                    instance = go.AddComponent<BrightnessSystem>();
                }
            }
            return instance;
        }
    }

    private const string BrightnessKey = "BrightnessValue";

    [Header("Brightness Range")]
    [Tooltip("Minimum exposure value (slider at 0.0).")]
    [SerializeField] private float minExposure = -1f;

    [Tooltip("Maximum exposure value (slider at 1.0).")]
    [SerializeField] private float maxExposure = 1f;

    private ColorAdjustments activeColorAdjustments;
    private Volume currentActiveVolume;
    private float baselineExposure;


    public float CurrentBrightness { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        CurrentBrightness = PlayerPrefs.GetFloat(BrightnessKey, 0.5f);
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        AutoFindAndRegisterVolume();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        AutoFindAndRegisterVolume();
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

    public void RegisterVolume(Volume sceneVolume)
    {
        if (sceneVolume == null || sceneVolume.profile == null)
        {
            return;
        }

        currentActiveVolume = sceneVolume;

        VolumeProfile runtimeProfile = Instantiate(sceneVolume.profile);
        sceneVolume.profile = runtimeProfile;
        if (!runtimeProfile.TryGet(out activeColorAdjustments))
        {
            activeColorAdjustments = runtimeProfile.Add<ColorAdjustments>(true);
        }

        baselineExposure = activeColorAdjustments.postExposure.value;   // this my code, im trying to fix shit about brightness

        activeColorAdjustments.postExposure.overrideState = true;
        ApplyBrightness(CurrentBrightness);
        
        Debug.Log($"[BrightnessSystem] Registered volume: {sceneVolume.name}, Current Brightness: {CurrentBrightness}");
    }

    public void UnregisterVolume(Volume sceneVolume)
    {
        if (currentActiveVolume == sceneVolume)
        {
            currentActiveVolume = null;
            activeColorAdjustments = null;
        }
    }

    public void SetBrightness(float value)
    {
        CurrentBrightness = value;
        PlayerPrefs.SetFloat(BrightnessKey, value);
        PlayerPrefs.Save();

        if (activeColorAdjustments == null)
        {
            AutoFindAndRegisterVolume();
        }

        ApplyBrightness(value);
    }

    private void ApplyBrightness(float value)
    {
        if (activeColorAdjustments == null)
        {
            AutoFindAndRegisterVolume();
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