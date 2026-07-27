using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerData
{
    public interface IInteractable
    {
        void Interact();
    }

    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Transform InteractorSource;
        [SerializeField] private float InteractRange;

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Transform source = InteractorSource != null ? InteractorSource : transform;
                Ray r = new Ray(source.position, source.forward);
                if (Physics.Raycast(r, out RaycastHit hitInfo, InteractRange))
                {
                    if (hitInfo.transform.TryGetComponent(out IInteractable interactable))
                    {
                        interactable.Interact();
                    }
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Transform source = InteractorSource != null ? InteractorSource : transform;
            Gizmos.color = Color.green;
            Vector3 rayEnd = source.position + (source.forward * InteractRange);
            Gizmos.DrawLine(source.position, rayEnd);
            Gizmos.DrawWireSphere(rayEnd, 0.15f);
        }
    }
}
