using UnityEngine;
using UnityEngine.AI;

public class ClickMovement : MonoBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private NavMeshAgent agent;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                agent.SetDestination(hit.point);

                Vector3 direction = hit.point - transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                    transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}