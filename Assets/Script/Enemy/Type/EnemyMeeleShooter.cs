using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enemy
{
    public class EnemyMeeleShooter : EnemyBrain, IMeele, IShooter
    {
        public enum MeeleShooterMode
        {
            Meele,
            Shooter
        }

        [Header("Meele Capability")]
        [SerializeField] private Animator animator;
        [SerializeField] private int damage = 10;
        [SerializeField] private float damageInterval = 2.0f;
        [SerializeField] private BoxCollider damageCollider;

        [Header("Shooter Capability")]
        [SerializeField] protected float fireRate = 2.0f;
        [SerializeField] protected Transform firePoint;
        [SerializeField] protected Transform firePoint2;
        [SerializeField] protected GameObject bulletPrefab;

        private Health playerHealthInBox;
        private float nextDamageTime;
        private HashSet<Collider> playerCollidersInBox = new HashSet<Collider>();
        private float nextFireTime;
        private MeeleShooterMode currentMode = MeeleShooterMode.Shooter;
        private procedural_animation.EnemyProceduralAnimator proceduralAnimator;

        public MeeleShooterMode CurrentMode => currentMode;
        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public Transform FirePoint2 => firePoint2;
        public GameObject BulletPrefab => bulletPrefab;

        protected override void Start()
        {
            base.Start();
            proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>() ?? GetComponent<procedural_animation.EnemyProceduralAnimator>();

            if (damageCollider != null)
            {
                damageCollider.isTrigger = true;
            }

            UpdateModeBasedOnDistance();
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

        public void ShootAttack()
        {
            if (isKnockedBack) return;

            if (proceduralAnimator != null && playerTarget != null)
            {
                proceduralAnimator.SetLookTarget(playerTarget);
            }
            StartCoroutine(ShootSequence());
        }

        protected virtual IEnumerator ShootSequence()
        {
            Shoot(firePoint);

            if (firePoint2 != null)
            {
                yield return new WaitForSeconds(fireRate * 0.5f);
                Shoot(firePoint2);
            }
        }

        protected virtual void Shoot(Transform point)
        {
            if (bulletPrefab == null || point == null || playerTarget == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(playerTarget.position - point.position);
            Instantiate(bulletPrefab, point.position, rotation);
        }

        protected override void Update()
        {
            base.Update();

            if (isKnockedBack) return;

            UpdateModeBasedOnDistance();

            if (currentMode == MeeleShooterMode.Meele)
            {
                if (CheckPlayerInDamageCollider())
                {
                    if (Time.time >= nextDamageTime)
                    {
                        playerHealthInBox.TakeDamage(damage);
                        nextDamageTime = Time.time + damageInterval;
                    }
                }
            }

            if (currentMode == MeeleShooterMode.Shooter && IsPlayerDetected() && IsPlayerInAttackRange() && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootAttack();
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

        private void UpdateModeBasedOnDistance()
        {
            if (playerTarget == null) return;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance <= meeleRange)
            {
                if (currentMode != MeeleShooterMode.Meele)
                {
                    SwitchMode(MeeleShooterMode.Meele);
                }
            }
            else
            {
                if (currentMode != MeeleShooterMode.Shooter)
                {
                    SwitchMode(MeeleShooterMode.Shooter);
                }
            }
        }

        private void SwitchMode(MeeleShooterMode newMode)
        {
            currentMode = newMode;

            if (newMode == MeeleShooterMode.Shooter)
            {
                StopAttack();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsPlayerCollider(other, out Health health))
            {
                playerCollidersInBox.Add(other);
                playerHealthInBox = health;

                if (currentMode == MeeleShooterMode.Meele && Time.time >= nextDamageTime && !isKnockedBack)
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