using System.Collections;
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
        [SerializeField] private float fireRate = 2.0f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePoint2;
        [SerializeField] private GameObject bulletPrefab;

        private Health playerHealthInBox;
        private float damageTimer;
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
            if (proceduralAnimator != null && playerTarget != null)
            {
                proceduralAnimator.SetLookTarget(playerTarget);
            }
            StartCoroutine(ShootSequence());
        }

        private IEnumerator ShootSequence()
        {
            Shoot(firePoint);

            if (firePoint2 != null)
            {
                yield return new WaitForSeconds(fireRate * 0.5f);
                Shoot(firePoint2);
            }
        }

        private void Shoot(Transform point)
        {
            if (bulletPrefab == null || point == null || playerTarget == null)
                return;

            Quaternion rotation = Quaternion.LookRotation(playerTarget.position - point.position);
            Instantiate(bulletPrefab, point.position, rotation);
        }

        protected override void Update()
        {
            base.Update();

            UpdateModeBasedOnDistance();

            if (currentMode == MeeleShooterMode.Meele && playerHealthInBox != null)
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= damageInterval)
                {
                    playerHealthInBox.TakeDamage(damage);
                    damageTimer = 0f;
                }
            }

            if (currentMode == MeeleShooterMode.Shooter && IsPlayerDetected() && IsPlayerInAttackRange() && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootAttack();
            }
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
            if (other.CompareTag("Player") || other.GetComponent<Health>() != null)
            {
                Health health = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
                if (health != null)
                {
                    playerHealthInBox = health;
                    damageTimer = 0f;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") || (playerHealthInBox != null && (other.GetComponent<Health>() == playerHealthInBox || other.GetComponentInParent<Health>() == playerHealthInBox)))
            {
                playerHealthInBox = null;
                damageTimer = 0f;
            }
        }
    }
}