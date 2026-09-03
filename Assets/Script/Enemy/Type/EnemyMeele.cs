using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyMeele : EnemyBrain, IMeele
    {
        [Header("Meele Settings")]
        [SerializeField] private Animator animator;
        [SerializeField] private int damage = 10;
        [SerializeField] private float damageInterval = 2.0f;
        [SerializeField] private BoxCollider damageCollider;

        private float nextDamageTime;
        private procedural_animation.EnemyProceduralAnimator proceduralAnimator;

        protected override void Start()
        {
            base.Start();
            proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>() ?? GetComponent<procedural_animation.EnemyProceduralAnimator>();

            if (damageCollider != null)
            {
                damageCollider.isTrigger = true;
            }
        }

        public void MeeleAttack()
        {
            if (isKnockedBack) return;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
                animator.SetBool("Attack", true);
            }

            if (proceduralAnimator != null)
            {
                proceduralAnimator.SetAttacking(true, PlayerTarget);
            }
        }

        public void StopAttack()
        {
            if (animator != null)
            {
                animator.SetBool("Attack", false);
            }

            if (proceduralAnimator != null)
            {
                proceduralAnimator.SetAttacking(false);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (isKnockedBack) return;

            // Check if player is strictly inside damageCollider
            Health playerHealth = GetPlayerInDamageCollider();
            if (playerHealth != null && playerHealth.CurrentHealth > 0)
            {
                if (Time.time >= nextDamageTime)
                {
                    playerHealth.TakeDamage(damage);
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }

        private Health GetPlayerInDamageCollider()
        {
            if (damageCollider == null || !damageCollider.enabled || !damageCollider.gameObject.activeInHierarchy)
            {
                return null;
            }

            Vector3 center = damageCollider.transform.TransformPoint(damageCollider.center);
            Vector3 halfExtents = Vector3.Scale(damageCollider.size, damageCollider.transform.lossyScale) * 0.5f;
            halfExtents = new Vector3(Mathf.Abs(halfExtents.x), Mathf.Abs(halfExtents.y), Mathf.Abs(halfExtents.z));

            Collider[] hits = Physics.OverlapBox(center, halfExtents, damageCollider.transform.rotation, ~0, QueryTriggerInteraction.Collide);

            foreach (var hit in hits)
            {
                if (hit == null) continue;
                if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

                if (IsPlayerCollider(hit, out Health health))
                {
                    return health;
                }
            }

            return null;
        }

        private bool IsPlayerCollider(Collider col, out Health health)
        {
            health = null;
            if (col == null) return false;

            if (col.CompareTag("Player") || col.transform.root.CompareTag("Player"))
            {
                health = col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
                if (health != null && !(health is EnemyHealth))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnDrawGizmosSelected()
        {
            if (damageCollider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix = Matrix4x4.TRS(
                    damageCollider.transform.TransformPoint(damageCollider.center),
                    damageCollider.transform.rotation,
                    Vector3.Scale(damageCollider.size, damageCollider.transform.lossyScale)
                );
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            }
        }
    }
}