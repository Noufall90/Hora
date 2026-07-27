using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerData
{
    public class PlayerAnimAttack : MonoBehaviour
    {
        [Header("Components")]
        public float maxComboDelay = 0.5f;
        private float slashDuration = 0.4f;
        [SerializeField] private float shootDuration = 0.4f;

        [Header("Barang Dissolve")]
        [SerializeField] private GameObject dissolvePedang;
        [SerializeField] private GameObject dissolvePistol;
        [SerializeField] private GameObject[] dissolveObjects;
        [SerializeField] private float timeBeforeAction = 0.2f;
        [SerializeField] private float timeAfterAction = 0.2f;
        [SerializeField] private GameObject[] reverseDissolveObject;
        [SerializeField] private float reverseTimeBeforeAction = 0.2f;
        [SerializeField] private float reverseTimeAfterAction = 0.2f;

        private static readonly int DissolveHash = Shader.PropertyToID("_Dissolve");

        private PlayerController _playerController;
        private Animator _animator => (_playerController != null ? _playerController.Animator : (PlayerController.Instance != null ? PlayerController.Instance.Animator : null));

        private Coroutine _attackResetCoroutine;
        private Coroutine _shootResetCoroutine;
        private Coroutine _comboResetCoroutine;
        private int _comboStep = 0;

        private bool _isAttacking;
        private bool _isShooting;

        private Dictionary<GameObject, Coroutine> _activeDissolveCoroutines = new Dictionary<GameObject, Coroutine>();
        private Dictionary<GameObject, float> _currentDissolveValues = new Dictionary<GameObject, float>();

        private Dictionary<GameObject, Coroutine> _activeReverseDissolveCoroutines = new Dictionary<GameObject, Coroutine>();
        private Dictionary<GameObject, float> _currentReverseDissolveValues = new Dictionary<GameObject, float>();

        public bool IsAttacking => _isAttacking;
        public bool IsShooting => _isShooting;
        public bool IsAttackingOrShooting => _isAttacking || _isShooting;
        public bool IsAttackingOrShielding => IsAttackingOrShooting;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        private void Start()
        {
            if (_animator != null)
            {
                _animator.SetBool(PlayerController.IsIdleBoolHash, true);
            }

            InitializeDissolveObjects();
        }

        private void InitializeDissolveObjects()
        {
            if (dissolvePedang != null)
            {
                SetDissolveValue(dissolvePedang, 0f);
                _currentDissolveValues[dissolvePedang] = 0f;
                dissolvePedang.SetActive(false);
            }

            if (dissolvePistol != null)
            {
                SetDissolveValue(dissolvePistol, 0f);
                _currentDissolveValues[dissolvePistol] = 0f;
                dissolvePistol.SetActive(false);
            }

            if (dissolveObjects != null)
            {
                foreach (var obj in dissolveObjects)
                {
                    if (obj == null) continue;

                    SetDissolveValue(obj, 0f);
                    _currentDissolveValues[obj] = 0f;
                    obj.SetActive(false);
                }
            }

            if (reverseDissolveObject != null)
            {
                foreach (var obj in reverseDissolveObject)
                {
                    if (obj == null) continue;

                    SetDissolveValue(obj, 1f);
                    _currentReverseDissolveValues[obj] = 1f;
                    obj.SetActive(true);
                }
            }
        }

        public void HandleAttack()
        {
            if (_animator == null) return;

            _isAttacking = true;
            TriggerDissolveIn(dissolvePedang);

            _animator.SetBool(PlayerController.IsIdleBoolHash, false);

            if (_comboStep == 0)
            {
                _animator.ResetTrigger(PlayerController.AttackTriggerHash);
                _animator.ResetTrigger(PlayerController.AttackTriggerHash2);
                _animator.SetTrigger(PlayerController.AttackTriggerHash);
                _comboStep = 1;
            }
            else
            {
                _animator.ResetTrigger(PlayerController.AttackTriggerHash);
                _animator.ResetTrigger(PlayerController.AttackTriggerHash2);
                _animator.SetTrigger(PlayerController.AttackTriggerHash2);
                _comboStep = 0;
            }

            if (_attackResetCoroutine != null)
            {
                StopCoroutine(_attackResetCoroutine);
            }
            _attackResetCoroutine = StartCoroutine(ResetIdleAfterAttack(slashDuration));

            if (_comboResetCoroutine != null)
            {
                StopCoroutine(_comboResetCoroutine);
            }
            _comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(slashDuration + maxComboDelay));
        }

        public void HandleShoot()
        {
            if (_animator == null) return;

            _isShooting = true;
            TriggerDissolveIn(dissolvePistol);

            _animator.SetBool(PlayerController.IsIdleBoolHash, false);

            _animator.ResetTrigger(PlayerController.ShootTriggerHash);
            _animator.SetTrigger(PlayerController.ShootTriggerHash);

            if (_shootResetCoroutine != null)
            {
                StopCoroutine(_shootResetCoroutine);
            }
            _shootResetCoroutine = StartCoroutine(ResetIdleAfterShoot(shootDuration));
        }

        public void OnShootComplete()
        {
            _isShooting = false;

            if (_shootResetCoroutine != null)
            {
                StopCoroutine(_shootResetCoroutine);
                _shootResetCoroutine = null;
            }

            if (!_isAttacking)
            {
                if (_animator != null)
                {
                    _animator.SetBool(PlayerController.IsIdleBoolHash, true);
                }
                TriggerDissolveOut(dissolvePistol);
            }
            else
            {
                StartDissolveOutSingle(dissolvePistol);
            }
        }

        private IEnumerator ResetIdleAfterShoot(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnShootComplete();
        }

        public void OnAttackComplete()
        {
            _isAttacking = false;

            if (_attackResetCoroutine != null)
            {
                StopCoroutine(_attackResetCoroutine);
                _attackResetCoroutine = null;
            }

            if (!_isShooting)
            {
                if (_animator != null)
                {
                    _animator.SetBool(PlayerController.IsIdleBoolHash, true);
                }
                TriggerDissolveOut(dissolvePedang);
            }
            else
            {
                StartDissolveOutSingle(dissolvePedang);
            }
        }

        public void CancelAttackAndShoot()
        {
            _isAttacking = false;
            _isShooting = false;

            if (_attackResetCoroutine != null)
            {
                StopCoroutine(_attackResetCoroutine);
                _attackResetCoroutine = null;
            }

            if (_shootResetCoroutine != null)
            {
                StopCoroutine(_shootResetCoroutine);
                _shootResetCoroutine = null;
            }

            TriggerDissolveOut();
        }

        private IEnumerator ResetIdleAfterAttack(float delay)
        {
            yield return new WaitForSeconds(delay);
            OnAttackComplete();
        }

        private IEnumerator ResetComboAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _comboStep = 0;
            _comboResetCoroutine = null;
        }

        private void TriggerDissolveIn(GameObject targetWeapon = null)
        {
            TriggerReverseDissolveIn();

            // Sembunyikan senjata lawan secara langsung agar tidak pernah muncul bersamaan
            if (targetWeapon == dissolvePedang)
            {
                ForceHideWeapon(dissolvePistol);
                if (dissolvePedang != null)
                {
                    StartDissolveInSingle(dissolvePedang);
                }
            }
            else if (targetWeapon == dissolvePistol)
            {
                ForceHideWeapon(dissolvePedang);
                if (dissolvePistol != null)
                {
                    StartDissolveInSingle(dissolvePistol);
                }
            }

            // Memunculkan dissolveObjects lain jika ada (kecuali pedang & pistol)
            if (dissolveObjects != null)
            {
                foreach (var obj in dissolveObjects)
                {
                    if (obj == null) continue;
                    if (obj == dissolvePedang || obj == dissolvePistol) continue;

                    StartDissolveInSingle(obj);
                }
            }
        }

        private void TriggerDissolveOut(GameObject targetWeapon = null)
        {
            TriggerReverseDissolveOut();

            if (targetWeapon != null)
            {
                StartDissolveOutSingle(targetWeapon);
            }

            if (dissolveObjects != null)
            {
                foreach (var obj in dissolveObjects)
                {
                    if (obj == null) continue;
                    if (obj == dissolvePedang || obj == dissolvePistol) continue;

                    StartDissolveOutSingle(obj);
                }
            }
        }

        private void ForceHideWeapon(GameObject obj)
        {
            if (obj == null) return;
            if (_activeDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
                _activeDissolveCoroutines.Remove(obj);
            }
            _currentDissolveValues[obj] = 0f;
            SetDissolveValue(obj, 0f);
            obj.SetActive(false);
        }

        private void StartDissolveInSingle(GameObject obj)
        {
            if (obj == null) return;
            if (_activeDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            _activeDissolveCoroutines[obj] = StartCoroutine(AnimateDissolve(obj, 1f, timeBeforeAction, false));
        }

        private void StartDissolveOutSingle(GameObject obj)
        {
            if (obj == null) return;
            if (_activeDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            _activeDissolveCoroutines[obj] = StartCoroutine(AnimateDissolve(obj, 0f, timeAfterAction, true));
        }

        private void TriggerReverseDissolveIn()
        {
            if (reverseDissolveObject == null) return;

            foreach (var obj in reverseDissolveObject)
            {
                if (obj == null) continue;

                if (_activeReverseDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
                {
                    StopCoroutine(running);
                }

                _activeReverseDissolveCoroutines[obj] = StartCoroutine(AnimateReverseDissolve(obj, 0f, reverseTimeBeforeAction, deactivateOnComplete: true));
            }
        }

        private void TriggerReverseDissolveOut()
        {
            if (reverseDissolveObject == null) return;

            foreach (var obj in reverseDissolveObject)
            {
                if (obj == null) continue;

                if (_activeReverseDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
                {
                    StopCoroutine(running);
                }

                _activeReverseDissolveCoroutines[obj] = StartCoroutine(AnimateReverseDissolve(obj, 1f, reverseTimeAfterAction, deactivateOnComplete: false));
            }
        }

        private IEnumerator AnimateDissolve(GameObject obj, float targetValue, float duration, bool deactivateOnComplete)
        {
            obj.SetActive(true);

            float startValue = _currentDissolveValues.TryGetValue(obj, out float val) ? val : 0f;

            if (duration <= 0f)
            {
                _currentDissolveValues[obj] = targetValue;
                SetDissolveValue(obj, targetValue);
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float current = Mathf.Lerp(startValue, targetValue, elapsed / duration);
                    _currentDissolveValues[obj] = current;
                    SetDissolveValue(obj, current);
                    yield return null;
                }
                _currentDissolveValues[obj] = targetValue;
                SetDissolveValue(obj, targetValue);
            }

            if (deactivateOnComplete && targetValue <= 0f)
            {
                obj.SetActive(false);
            }

            _activeDissolveCoroutines.Remove(obj);
        }

        private IEnumerator AnimateReverseDissolve(GameObject obj, float targetValue, float duration, bool deactivateOnComplete)
        {
            obj.SetActive(true);

            float startValue = _currentReverseDissolveValues.TryGetValue(obj, out float val) ? val : 1f;

            if (duration <= 0f)
            {
                _currentReverseDissolveValues[obj] = targetValue;
                SetDissolveValue(obj, targetValue);
            }
            else
            {
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float current = Mathf.Lerp(startValue, targetValue, elapsed / duration);
                    _currentReverseDissolveValues[obj] = current;
                    SetDissolveValue(obj, current);
                    yield return null;
                }
                _currentReverseDissolveValues[obj] = targetValue;
                SetDissolveValue(obj, targetValue);
            }

            if (deactivateOnComplete && targetValue <= 0f)
            {
                obj.SetActive(false);
            }

            _activeReverseDissolveCoroutines.Remove(obj);
        }

        private void SetDissolveValue(GameObject obj, float value)
        {
            if (obj == null) return;

            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer rend = renderers[i];
                if (rend == null) continue;

                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                rend.GetPropertyBlock(mpb);
                mpb.SetFloat(DissolveHash, value);
                rend.SetPropertyBlock(mpb);

                Material[] mats = rend.materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    if (mats[j] != null && mats[j].HasProperty(DissolveHash))
                    {
                        mats[j].SetFloat(DissolveHash, value);
                    }
                }
            }
        }
    }
}