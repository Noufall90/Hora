using System.Collections;
using UnityEngine;
using TMPro;

namespace Enemy
{
    public class HealthDummy : EnemyHealth
    {
        [Header("Dummy Health Threshold & Reset Settings")]
        [SerializeField] private int minHealthThreshold = 10;
        [SerializeField] private float autoResetDelay = 3f;

        [Header("Damage Text Settings (TMP)")]
        [SerializeField] private TMP_Text damageText;
        [SerializeField] private float damageTextDuration = 1f;

        private float _idleResetTimer = 0f;
        private int _totalDamageAccumulated = 0;
        private Coroutine _damageTextCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            _totalDamageAccumulated = 0;
            if (damageText != null)
            {
                damageText.gameObject.SetActive(false);
            }
        }

        public override void TakeDamage(int amount)
        {
            if (currentHealth <= 0) return;

            _idleResetTimer = autoResetDelay;
            base.TakeDamage(amount);
            ShowDamageText(amount);

            if (currentHealth <= minHealthThreshold)
            {
                ResetHealthToMax();
            }
        }

        protected override void Die()
        {
            ResetHealthToMax();
        }

        private void Update()
        {
            if (currentHealth < maxHealth)
            {
                if (_idleResetTimer > 0f)
                {
                    _idleResetTimer -= Time.deltaTime;
                    if (_idleResetTimer <= 0f)
                    {
                        ResetHealthToMax();
                    }
                }
            }
        }

        public void ResetHealthToMax()
        {
            currentHealth = maxHealth;
            _idleResetTimer = 0f;

            HealthEnemyUI healthUI = GetComponent<HealthEnemyUI>() ?? GetComponentInChildren<HealthEnemyUI>() ?? GetComponentInParent<HealthEnemyUI>();
            if (healthUI != null)
            {
                healthUI.UpdateHealthUI();
            }
        }

        private void ShowDamageText(int damageAmount)
        {
            if (damageText == null) return;

            _totalDamageAccumulated += damageAmount;
            damageText.text = _totalDamageAccumulated.ToString();
            damageText.gameObject.SetActive(true);

            if (_damageTextCoroutine != null)
            {
                StopCoroutine(_damageTextCoroutine);
            }

            _damageTextCoroutine = StartCoroutine(HideDamageTextRoutine());
        }

        private IEnumerator HideDamageTextRoutine()
        {
            yield return new WaitForSeconds(damageTextDuration);

            _totalDamageAccumulated = 0;

            if (damageText != null)
            {
                damageText.gameObject.SetActive(false);
            }
            _damageTextCoroutine = null;
        }
    }
}
