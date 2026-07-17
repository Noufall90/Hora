using UnityEngine;

namespace Enemy
{
    public class EnemyMeeleShooter : EnemyBrain, IMeele, IShooter
    {
        [Header("Meele Capability")]
        [SerializeField] private float meeleAttackRange = 2f;

        [Header("Shooter Capability")]
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private Transform firePoint;
        [SerializeField] private GameObject bulletPrefab;

        public float MeeleAttackRange => meeleAttackRange;
        public float FireRate => fireRate;
        public Transform FirePoint => firePoint;
        public GameObject BulletPrefab => bulletPrefab;

        public void MeeleAttack()
        {
            Debug.Log($"{gameObject.name} memukul target karena terlalu dekat!");
        }

        public void ShootAttack()
        {
            Debug.Log($"{gameObject.name} menembak target dari jarak aman!");
        }
    }
}