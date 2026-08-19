using UnityEngine;
using UnityEngine.UI;
using System;

namespace PlayerData
{
    public class HealthPlayerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("UI Elements")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image shieldBar;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindObjectOfType<PlayerHealth>();
            }
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnShieldChanged += UpdateShieldUI;

                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                UpdateShieldUI(playerHealth.CurrentShield, playerHealth.MaxShield);
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
                playerHealth.OnShieldChanged -= UpdateShieldUI;
            }
        }

        private void Update()
        {
            if (playerHealth == null) return;

            // Testing Health (J = -10 HP (Direct), K = +10 HP)
            if (Input.GetKeyDown(KeyCode.K))
            {
                playerHealth.TakeDirectHealthDamage(10);
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                playerHealth.Heal(10);
            }

            // Testing Shield (O = -10 Shield, P = +10 Shield)
            if (Input.GetKeyDown(KeyCode.O))
            {
                playerHealth.AddShield(-10f);
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                playerHealth.AddShield(10f);
            }
        }

        private void UpdateHealthUI(int currentHealth, int maxHealth)
        {
            if (healthBar != null && maxHealth > 0)
            {
                healthBar.fillAmount = (float)currentHealth / maxHealth;
            }
        }

        private void UpdateShieldUI(float currentShield, float maxShield)
        {
            if (shieldBar != null && maxShield > 0)
            {
                shieldBar.fillAmount = currentShield / maxShield;
            }
        }
}
}