using UnityEngine;

namespace PlayerWeapons
{
    public class ProjectilePlayer : MonoBehaviour
    {
        [Header("Projectile Settings")]
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private int defaultDamage = 10;

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
                    if (IsPlayer(hit.collider)) continue;

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

            // Ignore shooter (Player)
            if (IsPlayer(other))
                return;

            // If it's a trigger collider, ignore if it doesn't have a Health component
            if (other.isTrigger)
            {
                Health targetHealth = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
                if (targetHealth == null) return;
            }

            hasHit = true;

            if (IsEnemy(other))
            {
                Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
                if (health != null)
                {
                    int damage = GetDamage();
                    health.TakeDamage(damage);

                    if (CameraShake.Instance != null)
                    {
                        CameraShake.Instance.CameraShaked(5f, 0.1f);
                    }
                }
            }

            Destroy(gameObject);
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

        private bool IsEnemy(Collider other)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if (other.CompareTag("Enemy") || other.transform.root.CompareTag("Enemy"))
                return true;

            if (enemyLayer != -1 && (other.gameObject.layer == enemyLayer || other.transform.root.gameObject.layer == enemyLayer))
                return true;

            Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (health != null && !(health is PlayerData.PlayerHealth))
                return true;

            return false;
        }

        private int GetDamage()
        {
            int damage = defaultDamage;

            if (WeaponsManager.Instance != null)
            {
                float pistolDamage =
                    WeaponsManager.Instance.CurrentPistolData.damagePistol;

                if (pistolDamage > 0)
                {
                    damage = Mathf.RoundToInt(pistolDamage);
                }
            }

            return damage;
        }
    }
}