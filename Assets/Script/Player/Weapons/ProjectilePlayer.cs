using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerWeapons
{
    public class ProjectilePlayer : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifeTime = 3f;
        [SerializeField] private LayerMask targetLayer;

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
            int otherLayer = 1 << other.gameObject.layer;
            int rootLayer = 1 << other.transform.root.gameObject.layer;
            if ((targetLayer.value & otherLayer) == 0 && (targetLayer.value & rootLayer) == 0)
                return;

            Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();

            if (health != null)
            {
                int damage = 10;
                if (WeaponsManager.Instance != null)
                {
                    float pistolDmg = WeaponsManager.Instance.CurrentPistolData.damagePistol;
                    if (pistolDmg > 0)
                    {
                        damage = Mathf.RoundToInt(pistolDmg);
                    }
                }

                health.TakeDamage(damage);
                Debug.Log("Bullet hit " + other.name);
            }

            Destroy(gameObject);
        }
    }
}