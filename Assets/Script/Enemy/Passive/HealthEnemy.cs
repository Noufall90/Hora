using UnityEngine;
using System;

namespace Enemy
{
    public class EnemyHealth : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 100;
        private int currentHealth;

        public event Action OnDeath;
        public event Action<int> OnDamageTaken;

        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;

        private void OnEnable()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth -= amount;
            OnDamageTaken?.Invoke(amount);

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            OnDeath?.Invoke();
            // Logika default kematian (Object pooling / Destroy)
            Destroy(gameObject);
        }
    }
}