using UnityEngine;

namespace Enemy
{
    public class EnemyBoss : EnemyBrain, IMeele, IShooter
    {
        [Header("Meele Capability")]
        [SerializeField] private float meeleAttackRange = 4f;

        [Header("Shooter Capability")]
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform firePoint2;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Bomber Capability")]
        [SerializeField] private float explosionRadius = 8f;

        public float MeeleAttackRange => meeleAttackRange;
        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public Transform FirePoint2 => firePoint2;
        public GameObject BulletPrefab => bulletPrefab;
        public float ExplosionRadius => explosionRadius;

        public void MeeleAttack()
        {
            Debug.Log("Boss menghempaskan serangan dekat!");
        }

        public void ShootAttack()
        {
            Debug.Log("Boss melakukan rentetan tembakan!");
        }

        public void Explode()
        {
            Debug.Log("Boss memicu ledakan area raksasa!");
        }
    }
}