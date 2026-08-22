using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            if (!rb.isKinematic)
            {
                rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            }
        }
    }

    private void Start()
    {
        previousPosition = transform.position;

        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        Destroy(gameObject, lifeTime);
    }

    private void FixedUpdate()
    {
        if (hasHit) return;

        Vector3 currentPosition = transform.position;
        Vector3 displacement = currentPosition - previousPosition;
        float distance = displacement.magnitude;

        if (distance > 0.0001f)
        {
            Vector3 direction = displacement / distance;
            RaycastHit[] hits = Physics.RaycastAll(previousPosition, direction, distance + 0.05f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                if (IsEnemy(hit.collider)) continue;

                if (hit.collider.isTrigger)
                {
                    Health h = hit.collider.GetComponent<Health>() ?? hit.collider.GetComponentInParent<Health>();
                    if (h == null) continue;
                }

                ProcessHit(hit.collider);
                break;
            }
        }

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        ProcessHit(other);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ProcessHit(collision.collider);
    }

    private void ProcessHit(Collider other)
    {
        if (hasHit || other == null) return;

        if (IsEnemy(other))
            return;

        if (other.isTrigger)
        {
            Health triggerHealth = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (triggerHealth == null) return;
        }

        hasHit = true;

        if (IsPlayer(other))
        {
            Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("Bullet hit " + other.name + " causing " + damage + " damage");
            }
        }

        Destroy(gameObject);
    }

    private bool IsEnemy(Collider other)
    {
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
            return true;
        if (enemyLayer != -1 && (other.gameObject.layer == enemyLayer || other.transform.root.gameObject.layer == enemyLayer))
            return true;
        return false;
    }

    private bool IsPlayer(Collider other)
    {
        int playerLayer = LayerMask.NameToLayer("Player");
        if (other.CompareTag("Player") || other.transform.root.CompareTag("Player"))
            return true;
        if (playerLayer != -1 && (other.gameObject.layer == playerLayer || other.transform.root.gameObject.layer == playerLayer))
            return true;
        return false;
    }
}