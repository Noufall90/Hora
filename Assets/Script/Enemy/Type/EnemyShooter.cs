using UnityEngine;

namespace Enemy
{
    public class EnemyShooter : EnemyBrain, IShooter
    {
        [Header("Shooter Settings")]
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePoint2;
        [SerializeField] private GameObject bulletPrefab;

        private Transform target;
        private float nextFireTime;
        private bool useFirstFirePoint = true;

        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public GameObject BulletPrefab => bulletPrefab;

        protected override void Update()
        {
            base.Update();

            // Gunakan metode deteksi dari base class (sudah pakai view cone)
            if (IsPlayerDetected())
            {
                target = playerTarget;
            }
            else
            {
                target = null;
                return;
            }

            // Menghadap ke target (hanya rotasi Y)
            Vector3 lookPos = target.position - transform.position;
            lookPos.y = 0;
            if (lookPos != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookPos);

            // Gunakan IsPlayerInAttackRange() untuk menembak (view cone + jarak)
            if (IsPlayerInAttackRange() && Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + fireRate;
                ShootAttack();
            }
        }

        public void ShootAttack()
        {
            if (bulletPrefab == null || target == null)
                return;

            Transform currentFirePoint = useFirstFirePoint ? firePoint : firePoint2;
            if (currentFirePoint == null)
                currentFirePoint = firePoint != null ? firePoint : firePoint2;

            if (currentFirePoint != null)
            {
                Quaternion rotation = Quaternion.LookRotation(target.position - currentFirePoint.position);
                Instantiate(bulletPrefab, currentFirePoint.position, rotation);
            }

            useFirstFirePoint = !useFirstFirePoint;
        }
    }
}