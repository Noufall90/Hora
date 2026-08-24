using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class LookAtController : MonoBehaviour
{
    [Header("Target Settings")]
    public LayerMask targetLayer;
    public float headWeight = 1f;
    public float bodyWeight = 0.5f;

    [Header("Smooth Settings")]
    public float smoothSpeed = 3f;

    private Animator animator;
    private bool isActive = false;

    private float currentLookWeight = 0f;

    private Transform currentTarget;
    private Transform lastTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        float targetWeight = (isActive && currentTarget != null) ? 1f : 0f;

        currentLookWeight = Mathf.Lerp(currentLookWeight, targetWeight, Time.deltaTime * smoothSpeed);

        animator.SetLookAtWeight(currentLookWeight, bodyWeight, headWeight);

        if ((currentTarget != null || lastTarget != null) && currentLookWeight > 0.01f)
        {
            Transform target = currentTarget != null ? currentTarget : lastTarget;
            animator.SetLookAtPosition(target.position);
        }

        if (currentLookWeight <= 0.01f)
        {
            lastTarget = null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            isActive = true;
            currentTarget = other.transform;
            lastTarget = currentTarget;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            if (currentTarget == other.transform)
            {
                isActive = false;
                lastTarget = currentTarget;
                currentTarget = null;
            }
        }
    }
}
