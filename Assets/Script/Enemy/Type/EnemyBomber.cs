using UnityEngine;

namespace Enemy
{
    public class EnemyBomber : EnemyBrain
    {
        [Header("Bomber Settings")]
        [SerializeField] private float upwardForce = 3f;

        public float UpwardForce => upwardForce;

        public void Explode()
        {
            
            health.TakeDamage(health.MaxHealth);
        }
    }
}