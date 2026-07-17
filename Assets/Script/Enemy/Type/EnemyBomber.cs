using UnityEngine;

namespace Enemy
{
    public class EnemyBomber : EnemyBrain, IBomber
    {
        [Header("Bomber Settings")]
        [SerializeField] private float explosionRadius = 3f;

        public float ExplosionRadius => explosionRadius;

        public void Explode()
        {
            // Logika area damage ledakan di sekitar musuh
            Debug.Log($"{gameObject.name} meledakkan diri!");
            
            // Panggil fungsi kematian lewat komponen health setelah meledak
            health.TakeDamage(health.MaxHealth);
        }
    }
}