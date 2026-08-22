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
            if (!IsEnemy(other))
                return;

            Health health = other.GetComponent<Health>();
            if (health == null)
            {
                health = other.GetComponentInParent<Health>();
            }

            if (health != null)
            {
                int damage = GetDamage();

                health.TakeDamage(damage);
            }
            Destroy(gameObject);
        }

        private bool IsEnemy(Collider other)
        {
            int enemyLayer = LayerMask.NameToLayer("Enemy");

            if (enemyLayer == -1)
            {
                return false;
            }

            if (other.gameObject.layer == enemyLayer)
                return true;

            if (other.transform.root.gameObject.layer == enemyLayer)
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