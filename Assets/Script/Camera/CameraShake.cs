using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private CinemachineImpulseSource impulseSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void CameraShaked()
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
    }

    public void CameraShaked(float force)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
}