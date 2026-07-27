using UnityEngine;
using System;

namespace PlayerData
{
    public class PlayerHealth : Health
    {
        [Header("Shield Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private float sheildPower = 100f;
        [SerializeField] private float sheildPowerRegenRate = 20f;
        [SerializeField] private float shieldRegenDelay = 3f;

        private float _currentShieldPower;
        private float _regenTimer;

        private static readonly int IsDeathHash = Animator.StringToHash("IsDeath");

        public event Action<float, float> OnShieldChanged;
        public event Action OnShieldBroken;

        public float CurrentShield => _currentShieldPower;
        public float MaxShield => sheildPower;
        public bool IsShieldActive => _currentShieldPower > 0;

        protected override void OnEnable()
        {
            base.OnEnable();
            _currentShieldPower = sheildPower;
            _regenTimer = shieldRegenDelay;
            OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
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

                OnShieldChanged?.Invoke(_currentShieldPower, sheildPower);
            }

            if (remainingDamage > 0f)
            {
                base.TakeDamage((int)remainingDamage);
            }
        }

        protected override void Die()
        {
            if (_animator != null)
            {
                _animator.SetTrigger(IsDeathHash);
            }
            base.Die();
        }
    }
}