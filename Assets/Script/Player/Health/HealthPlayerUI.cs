using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using EasyTransition;

namespace PlayerData
{
    public class HealthPlayerUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerHealth playerHealth;

        [Header("Death Panel Settings")]
        [SerializeField] private GameObject deathPanel;
        [SerializeField] private TMP_Text coinText;
        [SerializeField] private float deathPanelDelay = 1.5f;

        [Header("Transition Settings")]
        public TransitionSettings transition;
        [SerializeField] private float startDelay = 0.5f;
        [SerializeField] private string sceneName = "Home";
        [SerializeField] private string targetSpawnID;

        [Header("UI Elements")]
        [SerializeField] private Image healthBar;
        [SerializeField] private Image shieldBar;

        private Coroutine _deathPanelCoroutine;
        private bool _isDeathHandled = false;

        private void Awake()
        {
            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>() ?? FindObjectOfType<PlayerHealth>();
            }

            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }

            if (playerHealth != null)
            {
                UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                UpdateShieldUI(playerHealth.CurrentShield, playerHealth.MaxShield);
            }
        }

        private void OnEnable()
        {
            _isDeathHandled = false;

            if (deathPanel != null)
            {
                deathPanel.SetActive(false);
            }

            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged += UpdateHealthUI;
                playerHealth.OnShieldChanged += UpdateShieldUI;
                playerHealth.OnDeath += HandlePlayerDeath;

                if (playerHealth.MaxHealth > 0)
                {
                    UpdateHealthUI(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                    UpdateShieldUI(playerHealth.CurrentShield, playerHealth.MaxShield);
                }
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.OnHealthChanged -= UpdateHealthUI;
                playerHealth.OnShieldChanged -= UpdateShieldUI;
                playerHealth.OnDeath -= HandlePlayerDeath;
            }

            if (_deathPanelCoroutine != null)
            {
                StopCoroutine(_deathPanelCoroutine);
                _deathPanelCoroutine = null;
            }
        }

        private void Update()
        {
            if (playerHealth == null) return;

            // Testing Health (K = -10 HP (Direct), L = +10 HP)
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

        private void HandlePlayerDeath()
        {
            if (_isDeathHandled) return;
            _isDeathHandled = true;

            if (_deathPanelCoroutine != null)
            {
                StopCoroutine(_deathPanelCoroutine);
            }
            _deathPanelCoroutine = StartCoroutine(ShowDeathPanelRoutine());
        }

        private IEnumerator ShowDeathPanelRoutine()
        {
            if (deathPanelDelay > 0f)
            {
                yield return new WaitForSecondsRealtime(deathPanelDelay);
            }

            if (deathPanel != null)
            {
                deathPanel.SetActive(true);
            }

            if (coinText != null)
            {
                int currentCoin = CoinCounter.Instance != null ? CoinCounter.Instance.Coin : 0;
                coinText.text = currentCoin.ToString();
            }

            _deathPanelCoroutine = null;
        }

        public void HomeButton()
        {
            Time.timeScale = 1f;

            if (!string.IsNullOrEmpty(targetSpawnID))
            {
                PointLocation.SetSpawnTarget(targetSpawnID, SceneManager.GetActiveScene().name);
            }

            if (TransitionManager.Instance() != null && transition != null)
            {
                TransitionManager.Instance().Transition(sceneName, transition, startDelay);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }
    }
}