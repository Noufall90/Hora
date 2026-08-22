using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
        }
    }

    private void Start()
    {
        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Focus check specifically on "Player" tag or "Player" layer
        int playerLayer = LayerMask.NameToLayer("Player");
        bool isPlayer = other.CompareTag("Player") || 
                        other.transform.root.CompareTag("Player") || 
                        other.gameObject.layer == playerLayer || 
                        other.transform.root.gameObject.layer == playerLayer;

        if (!isPlayer)
            return;

        Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
            Debug.Log("Bullet hit " + other.name + " causing " + damage + " damage");
        }

        Destroy(gameObject);
    }
}