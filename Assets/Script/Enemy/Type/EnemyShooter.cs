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

        private Transform target;
        private float nextFireTime;

        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public GameObject BulletPrefab => bulletPrefab;

        protected override void Update()
        {
            base.Update();

            if (IsPlayerDetected())
            {
                target = playerTarget;
            }
            else
            {
                target = null;
                return;
            }

            // Menghadap player
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;

            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookPos);

            // Mulai satu siklus tembakan
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

            Quaternion rotation = Quaternion.LookRotation(
                target.position - point.position);

            Instantiate(
                bulletPrefab,
                point.position,
                rotation);
        }

        public void ShootAttack()
        {
            StartCoroutine(ShootSequence());
        }
    }
}