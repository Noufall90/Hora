using UnityEngine;
using System;

namespace PlayerData
{
    public class PlayerHealth : Health
    {
        public static PlayerHealth Instance { get; private set; }

        [Header("Shield Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private GameObject shieldVisual;
        [SerializeField] private float sheildPower = 100f;
        [SerializeField] private float sheildPowerRegenRate = 20f;
        [SerializeField] private float shieldRegenDelay = 3f;
        private const string HealthKey = "PlayerHealth";
        private const string MaxHealthKey = "PlayerMaxHealth";
        private static bool hasPersistentHealth;

        private float _currentShieldPower;
        private float _regenTimer;

        private static readonly int IsDeathHash = Animator.StringToHash("IsDeath");

        public event Action<int, int> OnHealthChanged;
        public event Action<float, float> OnShieldChanged;
        public event Action OnShieldBroken;

        public float CurrentShield => _currentShieldPower;
        public float MaxShield => sheildPower;
        public bool IsShieldActive => _currentShieldPower > 0;

        private bool _hasLoadedHealth = false;

        protected override void OnEnable()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (hasPersistentHealth)
            {
                maxHealth = PlayerPrefs.GetInt(MaxHealthKey, maxHealth);
                currentHealth = PlayerPrefs.GetInt(HealthKey, maxHealth);
                _hasLoadedHealth = true;
            }
            else
            {
                currentHealth = maxHealth;
            }

            _currentShieldPower = sheildPower;
            _regenTimer = shieldRegenDelay;

            ResetDissolveValue();
            UpdateShieldVisual();

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
        }

        public void SaveHealth()
        {
            PlayerPrefs.SetInt(HealthKey, currentHealth);
            PlayerPrefs.SetInt(MaxHealthKey, maxHealth);
            PlayerPrefs.Save();

            hasPersistentHealth = true;
        }

        public void SetHealth(int health, int max)
        {
            maxHealth = max;
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            _hasLoadedHealth = true;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        public void ResetToFull()
        {
            currentHealth = maxHealth;
            _hasLoadedHealth = false;

            hasPersistentHealth = true;

            PlayerPrefs.SetInt(HealthKey, maxHealth);
            PlayerPrefs.SetInt(MaxHealthKey, maxHealth);
            PlayerPrefs.Save();

            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }

        private void ResetDissolveValue()
        {
            if (playerRenderers == null || playerRenderers.Length == 0)
            {
                playerRenderers = GetComponentsInChildren<Renderer>();
            }

            if (playerRenderers != null)
            {
                foreach (Renderer rend in playerRenderers)
                {
                    if (rend == null) continue;

                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(mpb);
                    mpb.SetFloat(DissolvePropertyHash, 1f);
                    rend.SetPropertyBlock(mpb);

                    foreach (Material mat in rend.materials)
                    {
                        if (mat != null && mat.HasProperty(DissolvePropertyHash))
                        {
                            mat.SetFloat(DissolvePropertyHash, 1f);
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (currentHealth <= 0) return;

            if (_regenTimer < shieldRegenDelay)
            {
                _regenTimer += Time.deltaTime;
            }
            else if (_currentShieldPower < sheildPower)
            {
                _currentShieldPower += sheildPowerRegenRate * Time.deltaTime;
                _currentShieldPower = Mathf.Min(_currentShieldPower, sheildPower);
                UpdateShieldVisual();
                OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
            }
        }

        public override void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            _regenTimer = 0f;

            float remainingDamage = amount;

            if (_currentShieldPower > 0)
            {
                if (_currentShieldPower >= remainingDamage)
                {
                    _currentShieldPower -= remainingDamage;
                    remainingDamage = 0f;
                }
                else
                {
                    remainingDamage -= _currentShieldPower;
                    _currentShieldPower = 0f;
                    OnShieldBroken?.Invoke();
                }

                if (SoundManager.Instance != null)
                {
                    SoundManager.Instance.PlaySound2D("Sheild_Damage");
                }

                UpdateShieldVisual();
                OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
            }

            if (remainingDamage > 0f)
            {
                base.TakeDamage((int)remainingDamage);
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }

            SaveHealth();
        }

        public void TakeDirectHealthDamage(int amount)
        {
            if (currentHealth <= 0) return;

            base.TakeDamage(amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            SaveHealth();
        }
        public void Heal(int amount)
        {
            if (currentHealth <= 0) return;

            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);

            OnHealthChanged?.Invoke(currentHealth, maxHealth);

            SaveHealth();
        }

        public void AddShield(float amount)
        {
            if (amount < 0f)
            {
                _regenTimer = 0f;
            }

            _currentShieldPower = Mathf.Clamp(_currentShieldPower + amount, 0f, sheildPower);
            UpdateShieldVisual();
            OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
            SaveHealth();
        }

        private void UpdateShieldVisual()
        {
            if (shieldVisual != null)
            {
                bool shouldBeActive = _currentShieldPower > 0;
                if (shieldVisual.activeSelf != shouldBeActive)
                {
                    shieldVisual.SetActive(shouldBeActive);
                }
            }
        }

        [Header("Death Dissolve Settings")]
        [SerializeField] private Renderer[] playerRenderers;
        [SerializeField] private float dissolveDuration = 2f;
        private static readonly int DissolvePropertyHash = Shader.PropertyToID("_Dissolve");

        protected override void Die()
        {
            InvokeOnDeath();

            if (_animator != null)
            {
                _animator.SetTrigger(IsDeathHash);
            }

            if (shieldVisual != null)
            {
                shieldVisual.SetActive(false);
            }

            // Nonaktifkan komponen pergerakan dan collider agar tidak dapat digerakkan saat mati
            PlayerController controller = GetComponent<PlayerController>();
            if (controller != null) controller.enabled = false;

            CharacterController charController = GetComponent<CharacterController>();
            if (charController != null) charController.enabled = false;

            StartCoroutine(DeathDissolveRoutine());
        }

        private System.Collections.IEnumerator DeathDissolveRoutine()
        {
            if (playerRenderers == null || playerRenderers.Length == 0)
            {
                playerRenderers = GetComponentsInChildren<Renderer>();
            }

            float elapsed = 0f;
            while (elapsed < dissolveDuration)
            {
                elapsed += Time.deltaTime;
                float dissolveVal = Mathf.Lerp(1f, 0f, elapsed / dissolveDuration);

                foreach (Renderer rend in playerRenderers)
                {
                    if (rend == null) continue;

                    MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                    rend.GetPropertyBlock(mpb);
                    mpb.SetFloat(DissolvePropertyHash, dissolveVal);
                    rend.SetPropertyBlock(mpb);

                    foreach (Material mat in rend.materials)
                    {
                        if (mat != null && mat.HasProperty(DissolvePropertyHash))
                        {
                            mat.SetFloat(DissolvePropertyHash, dissolveVal);
                        }
                    }
                }

                yield return null;
            }

            base.Die();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}