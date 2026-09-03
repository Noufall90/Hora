using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Missile : MonoBehaviour
{
    [Header("Missile Settings")]
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Explosion Effect")]
    [SerializeField] private GameObject explosionEffectPrefab;
    [SerializeField] private Vector3 explosionParticleOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float explosionEffectLifeTime = 3f;

    [Header("Explosion Settings")]
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardsModifier = 1f;
    [SerializeField] private int explosionDamage = 50;
    [SerializeField] private bool enableCameraShake = true;

    [Header("Layers")]
    [SerializeField] private LayerMask targetLayer; // Target layers that trigger explosion on impact
    [SerializeField] private LayerMask damageLayerMask; // LayerMask for objects taking explosion damage
    [SerializeField] private LayerMask physicsLayerMask; // LayerMask for Rigidbodies affected by explosion force

    [Header("Trail")]
    [SerializeField] private TrailRenderer trailRenderer;

    private Rigidbody rb;
    private Vector3 previousPosition;
    private bool hasExploded = false;

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

        if (trailRenderer == null)
            trailRenderer = GetComponent<TrailRenderer>();
    }

    private void Start()
    {
        previousPosition = transform.position;

        if (rb != null)
        {
            rb.velocity = transform.forward * speed;
        }

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = true;
        }

        Invoke(nameof(Explode), lifeTime);
    }

    private void FixedUpdate()
    {
        if (hasExploded) return;

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

                // Check if hit collider is in targetLayer or is a solid obstacle
                if ((targetLayer.value & (1 << hit.collider.gameObject.layer)) != 0 || !hit.collider.isTrigger)
                {
                    Explode();
                    break;
                }
            }
        }

        previousPosition = currentPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded || other == null) return;

        if (other.isTrigger)
        {
            Health targetHealth = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (targetHealth == null) return;
        }

        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0 || !other.isTrigger)
        {
            Explode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        Explode();
    }

    public void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        CancelInvoke(nameof(Explode));

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position + explosionParticleOffset, Quaternion.identity);
            Destroy(effect, explosionEffectLifeTime);
        }

        if (enableCameraShake && CameraShake.Instance != null)
        {
            CameraShake.Instance.CameraShaked();
        }

        ApplyExplosionEffects();

        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }

        Destroy(gameObject);
    }

    private void ApplyExplosionEffects()
    {
        // 1. Apply Explosion Damage to Health components in radius
        LayerMask maskDamage = damageLayerMask.value != 0 ? damageLayerMask : targetLayer;
        Collider[] damageColliders = Physics.OverlapSphere(transform.position, explosionRadius, maskDamage);
        HashSet<Health> damagedHealths = new HashSet<Health>();

        foreach (Collider hit in damageColliders)
        {
            if (hit == null) continue;
            Health health = hit.GetComponent<Health>() ?? hit.GetComponentInParent<Health>();

            if (health != null && !damagedHealths.Contains(health))
            {
                health.TakeDamage(explosionDamage);
                damagedHealths.Add(health);
            }
        }

        // 2. Apply Physics Explosion Force to Rigidbodies in radius
        if (physicsLayerMask.value != 0)
        {
            Collider[] physicsColliders = Physics.OverlapSphere(transform.position, explosionRadius, physicsLayerMask);
            HashSet<Rigidbody> affectedRigidbodies = new HashSet<Rigidbody>();

            foreach (Collider hit in physicsColliders)
            {
                if (hit == null) continue;
                Rigidbody hitRb = hit.attachedRigidbody;

                if (hitRb == null || affectedRigidbodies.Contains(hitRb))
                    continue;

                hitRb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardsModifier, ForceMode.Impulse);
                affectedRigidbodies.Add(hitRb);
            }
        }
    }

    private void OnDestroy()
    {
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}