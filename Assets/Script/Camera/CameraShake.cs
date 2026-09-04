using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Default Frequency")]
    [SerializeField] private float defaultFrequencyGain = 1f;

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingAmplitude;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        SetupNoiseComponent();
    }

    private void SetupNoiseComponent()
    {
        if (cinemachineVirtualCamera == null)
        {
            cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        }

        if (cinemachineVirtualCamera != null && noise == null)
        {
            noise = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }

    public void CameraShaked(float amplitude, float time, float frequency = 1f)
    {
        SetupNoiseComponent();

        if (noise == null)
        {
            Debug.LogWarning("[CameraShake] CinemachineBasicMultiChannelPerlin component tidak ditemukan pada Virtual Camera!");
            return;
        }

        noise.m_AmplitudeGain = amplitude;
        // Pastikan Frequency Gain tidak 0, agar noise bergetar/bergerak
        noise.m_FrequencyGain = frequency > 0 ? frequency : defaultFrequencyGain;

        startingAmplitude = amplitude;
        shakeTimerTotal = time;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;

            if (noise != null)
            {
                // Meredam getaran secara halus (smooth decay)
                noise.m_AmplitudeGain = Mathf.Lerp(startingAmplitude, 0f, 1f - (shakeTimer / shakeTimerTotal));
            }

            if (shakeTimer <= 0)
            {
                shakeTimer = 0;
                if (noise != null)
                {
                    noise.m_AmplitudeGain = 0f;
                }
            }
        }
    }
}