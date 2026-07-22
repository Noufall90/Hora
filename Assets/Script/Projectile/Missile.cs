using UnityEngine;

public class Missile : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask targetLayer;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Vector3 explosionOffset = Vector3.zero;
    [SerializeField] private float explosionLifeTime = 3f;

    [Header("Trail")]
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        rb.velocity = transform.forward * speed;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            health.TakeDamage(damage);
        }

        SpawnExplosion();

        Destroy(gameObject);
    }

    private void SpawnExplosion()
    {
        if (explosionEffectPrefab == null)
            return;

        GameObject effect = Instantiate(
            explosionEffectPrefab,
            transform.position + explosionOffset,
            Quaternion.identity);

        Destroy(effect, explosionLifeTime);
    }

    private void OnDestroy()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }
}