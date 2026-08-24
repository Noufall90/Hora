using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CamerShake : MonoBehaviour
{
    public static CamerShake Instance { get; private set; }

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

    public void CameraShake()
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
    }

    public void CameraShake(float force)
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse(force);
        }
    }
}