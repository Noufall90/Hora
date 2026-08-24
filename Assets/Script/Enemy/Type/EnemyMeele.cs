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

        private Health playerHealthInBox;
        private float nextDamageTime;
        private HashSet<Collider> playerCollidersInBox = new HashSet<Collider>();
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

            // Check if player is inside damageCollider
            if (CheckPlayerInDamageCollider())
            {
                if (Time.time >= nextDamageTime)
                {
                    playerHealthInBox.TakeDamage(damage);
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }

        private bool CheckPlayerInDamageCollider()
        {
            playerCollidersInBox.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            if (playerCollidersInBox.Count > 0)
            {
                if (playerHealthInBox != null && playerHealthInBox.CurrentHealth > 0)
                    return true;
            }

            if (damageCollider != null)
            {
                Vector3 center = damageCollider.transform.TransformPoint(damageCollider.center);
                Vector3 halfExtents = Vector3.Scale(damageCollider.size, damageCollider.transform.lossyScale) * 0.5f;
                Collider[] hits = Physics.OverlapBox(center, halfExtents, damageCollider.transform.rotation);

                foreach (var hit in hits)
                {
                    if (hit == null) continue;
                    if (hit.transform == transform || hit.transform.IsChildOf(transform)) continue;

                    if (IsPlayerCollider(hit, out Health health))
                    {
                        playerCollidersInBox.Add(hit);
                        playerHealthInBox = health;
                        return true;
                    }
                }
            }

            playerHealthInBox = null;
            return false;
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
            else
            {
                health = col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
                if (health != null && !(health is EnemyHealth))
                {
                    return true;
                }
            }

            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerCollider(other, out Health health))
            {
                playerCollidersInBox.Add(other);
                playerHealthInBox = health;

                if (Time.time >= nextDamageTime && !isKnockedBack)
                {
                    playerHealthInBox.TakeDamage(damage);
                    nextDamageTime = Time.time + damageInterval;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (playerCollidersInBox.Contains(other))
            {
                playerCollidersInBox.Remove(other);
            }

            playerCollidersInBox.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);

            if (playerCollidersInBox.Count == 0)
            {
                playerHealthInBox = null;
            }
        }
    }
}