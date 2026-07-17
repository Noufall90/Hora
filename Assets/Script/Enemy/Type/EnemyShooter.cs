using UnityEngine;

namespace Enemy
{
    public class EnemyShooter : EnemyBrain, IShooter
    {
        [Header("Shooter Settings")]
        [SerializeField] private float fireRate = 1.5f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;

        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public GameObject BulletPrefab => bulletPrefab;

        public void ShootAttack()
        {
            // Logika instansiasi peluru menuju target
            if (bulletPrefab != null && firePoint != null)
            {
                Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                Debug.Log($"{gameObject.name} menembak!");
            }
        }
    }
}