using UnityEngine;

public class MechaLookAt : MonoBehaviour
{
    [Header("IK References")]
    [SerializeField] private Transform lookTargetIK;

    [Header("Targeting Settings")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private float lookSpeed = 5f;

    void Update()
    {
        if (currentTarget != null && lookTargetIK != null)
        {
            Vector3 targetPosition = currentTarget.position;
            lookTargetIK.position = Vector3.Lerp(lookTargetIK.position, targetPosition, Time.deltaTime * lookSpeed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        currentTarget = newTarget;
    }

    public void ClearTarget()
    {
        currentTarget = null;
    }
}