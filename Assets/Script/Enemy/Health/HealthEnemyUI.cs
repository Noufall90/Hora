using UnityEngine;
using UnityEngine.UI;

namespace Enemy
{
    public class HealthEnemyUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private EnemyHealth enemyHealth;

        [Header("UI Elements")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image healthBarBackground;

        private Camera mainCamera;

        private void Awake()
        {
            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>() ?? GetComponentInParent<EnemyHealth>() ?? GetComponentInChildren<EnemyHealth>();
            }
        }

        private void OnEnable()
        {
            FindCamera();

            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>() ?? GetComponentInParent<EnemyHealth>() ?? GetComponentInChildren<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.OnDamageTaken += HandleDamageTaken;
                UpdateHealthUI();
            }
        }

        private void OnDisable()
        {
            if (enemyHealth != null)
            {
                enemyHealth.OnDamageTaken -= HandleDamageTaken;
            }
        }

        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                FindCamera();
            }

            if (mainCamera != null)
            {
                // Rotasi World Space Canvas agar selalu menghadap ke arah kamera
                transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
            }
        }

        private void FindCamera()
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        private void HandleDamageTaken(int damage)
        {
            UpdateHealthUI();
        }

        public void UpdateHealthUI()
        {
            if (enemyHealth == null || enemyHealth.MaxHealth <= 0) return;

            bool isAlive = enemyHealth.CurrentHealth > 0;
            float fill = Mathf.Clamp01((float)enemyHealth.CurrentHealth / enemyHealth.MaxHealth);

            if (healthBar != null)
            {
                healthBar.fillAmount = fill;
                healthBar.gameObject.SetActive(isAlive);
            }

            if (healthBarBackground != null)
            {
                healthBarBackground.gameObject.SetActive(isAlive);
            }
        }
    }
}