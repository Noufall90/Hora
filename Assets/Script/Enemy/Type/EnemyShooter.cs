using System.Collections;
using UnityEngine;

namespace Enemy
{
    public class EnemyShooter : EnemyBrain, IShooter
    {
        [Header("Shooter Settings")]
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePoint2;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Knockback Settings")]
        [SerializeField] private int requiredComboHits = 3;
        [SerializeField] private float comboResetTime = 1.5f;
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private float knockbackDuration = 0.3f;

        private Transform target;
        private float nextFireTime;
        private int currentComboHits = 0;
        private float comboResetTimer = 0f;
        private bool isKnockedBack = false;

        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public Transform FirePoint2 => firePoint2;
        public GameObject BulletPrefab => bulletPrefab;
        public bool IsKnockedBack => isKnockedBack;

        protected override void Start()
        {
            base.Start();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnDamageTakenHandler(int damageAmount)
        {
            base.OnDamageTakenHandler(damageAmount);

            currentComboHits++;
            comboResetTimer = comboResetTime;

            if (currentComboHits >= requiredComboHits)
            {
                currentComboHits = 0;
                ApplyKnockback();
            }
        }

        private void ApplyKnockback()
        {
            if (isKnockedBack) return;

            Vector3 knockbackDir = Vector3.zero;
            if (playerTarget != null)
            {
                knockbackDir = (transform.position - playerTarget.position);
                knockbackDir.y = 0f;
            }

            if (knockbackDir.sqrMagnitude < 0.001f)
            {
                knockbackDir = -transform.forward;
                knockbackDir.y = 0f;
            }

            knockbackDir.Normalize();
            StartCoroutine(KnockbackRoutine(knockbackDir));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction)
        {
            isKnockedBack = true;
            float elapsed = 0f;

            Animator anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Hit");
            }

            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float currentForce = Mathf.Lerp(knockbackForce, 0f, elapsed / knockbackDuration);
                Vector3 moveStep = direction * currentForce * Time.deltaTime;

                if (HasActiveNavMeshAgent)
                {
                    agent.Move(moveStep);
                }
                else
                {
                    transform.position += moveStep;
                }

                yield return null;
            }

            isKnockedBack = false;
        }

        protected override void Update()
        {
            base.Update();

            if (currentComboHits > 0)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                {
                    currentComboHits = 0;
                }
            }

            if (isKnockedBack) return;

            if (IsPlayerDetected())
            {
                target = playerTarget;
            }
            else
            {
                target = null;
                return;
            }

            RotateTowardsPlayer();

            if (IsPlayerInAttackRange() && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                StartCoroutine(ShootSequence());
            }
        }

        private IEnumerator ShootSequence()
        {
            Shoot(firePoint);

            yield return new WaitForSeconds(fireRate * 0.5f);

            Shoot(firePoint2);
        }

        private void Shoot(Transform point)
        {
            if (bulletPrefab == null || point == null || target == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(target.position - point.position);

            Instantiate(bulletPrefab, point.position, rotation);
        }

        public void ShootAttack()
        {
            if (isKnockedBack) return;
            StartCoroutine(ShootSequence());
        }
    }
}