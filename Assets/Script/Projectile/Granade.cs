using UnityEngine;
using System.Collections.Generic;

public class Granade : MonoBehaviour
{
    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Vector3 explosionParticleOffset = new Vector3(0f, 1f, 0f);

    [Header("Explosion Settings")]
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 1f;
    [SerializeField] private int explosionDamage = 50;
    [Header("Layer")]
    [SerializeField] private LayerMask damageLayerMask; // Digunakan untuk mencari objek yang bisa terkena hit/damage.
    [SerializeField] private LayerMask physicsLayerMask; // Digunakan untuk mencari objek yang bisa terkena efek physics (terlempar).
    [SerializeField] private LayerMask groundLayerMask;

    private float countdown;
    private bool hasExploded;
    private bool hasTouchedGround;

    private void Start()
    {
        countdown = explosionDelay;
    }

    private void Update()
    {
        if (hasExploded)
            return;

        if (hasTouchedGround)
        {
            countdown -= Time.deltaTime;

            if (countdown <= 0f)
            {
                Explode();
                hasExploded = true;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!hasTouchedGround)
        {
            hasTouchedGround = true;
        }
    }

    private void Explode()
    {
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position + explosionParticleOffset, Quaternion.identity);
            Destroy(effect, 3f);
        }

        ApplyExplosionEffects();
        Destroy(gameObject);
    }

    private void ApplyExplosionEffects()
    {
        // Apply Damage
        Collider[] damageColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageLayerMask);
        HashSet<Health> damagedHealths = new HashSet<Health>();

        foreach (Collider hit in damageColliders)
        {
            Health health = hit.GetComponentInParent<Health>();

            if (health != null && !damagedHealths.Contains(health))
            {
                health.TakeDamage(explosionDamage);
                damagedHealths.Add(health);
            }
        }

        // Apply Physics
        Collider[] physicsColliders = Physics.OverlapSphere(transform.position, explosionRadius, physicsLayerMask);
        HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();

        foreach (Collider hit in physicsColliders)
        {
            Rigidbody rb = hit.attachedRigidbody;

            if (rb == null || affectedRigidbodies.Contains(rb))
                continue;

            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
            affectedRigidbodies.Add(rb);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}