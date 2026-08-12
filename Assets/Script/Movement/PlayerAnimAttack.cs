using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerWeapons;

namespace PlayerData
{
    public class PlayerAnimAttack : MonoBehaviour
    {
        [Header("Components")]
        public float maxComboDelay = 0.5f;
        private float slashDuration = 0.4f;

        [Header("Barang Dissolve")]
        [SerializeField] private GameObject dissolvePedang;
        [SerializeField] private GameObject dissolvePistol;
        [SerializeField] private GameObject[] dissolveObjects;
        [SerializeField] private float timeBeforeAction = 0.2f;
        [SerializeField] private float timeAfterAction = 0.2f;
        [SerializeField] private GameObject[] reverseDissolveObject;
        [SerializeField] private float reverseTimeBeforeAction = 0.2f;
        [SerializeField] private float reverseTimeAfterAction = 0.2f;

        [Header("Weapon Data Providers")]
        [SerializeField] private WeaponsManager weaponsManager;
        [SerializeField] private WeaponsActions weaponsActions;
        [SerializeField] private PistolItem pistolItem;
        [SerializeField] private MeeleItem meeleItem;

        private static readonly int DissolveHash = Shader.PropertyToID("_Dissolve");

        public static PlayerAnimAttack Instance { get; private set; }

        private PlayerController _playerController;
        private Animator _animator => (_playerController != null ? _playerController.Animator : (PlayerController.Instance != null ? PlayerController.Instance.Animator : null));

        private Coroutine _attackResetCoroutine;
        private Coroutine _shootResetCoroutine;
        private Coroutine _comboResetCoroutine;
        private int _comboStep = 0;

        private bool _isAttacking;
        private bool _isShooting;
        private Transform _currentTargetEnemy;

        private Dictionary<GameObject, Coroutine> _activeDissolveCoroutines = new Dictionary<GameObject, Coroutine>();
        private Dictionary<GameObject, float> _currentDissolveValues = new Dictionary<GameObject, float>();

        private Dictionary<GameObject, Coroutine> _activeReverseDissolveCoroutines = new Dictionary<GameObject, Coroutine>();
        private Dictionary<GameObject, float> _currentReverseDissolveValues = new Dictionary<GameObject, float>();

        public bool IsAttacking => _isAttacking;
        public bool IsShooting => _isShooting;
        public bool IsAttackingOrShooting => _isAttacking || _isShooting;
        public bool IsAttackingOrShielding => IsAttackingOrShooting;

        public GameObject GetActiveMeleeObject()
        {
            if (weaponsManager != null && weaponsManager.CurrentMeleeObject != null)
                return weaponsManager.CurrentMeleeObject;
            if (WeaponsManager.Instance != null && WeaponsManager.Instance.CurrentMeleeObject != null)
                return WeaponsManager.Instance.CurrentMeleeObject;
            return dissolvePedang;
        }

        public GameObject GetActivePistolObject()
        {
            if (weaponsManager != null && weaponsManager.CurrentPistolObject != null)
                return weaponsManager.CurrentPistolObject;
            if (WeaponsManager.Instance != null && WeaponsManager.Instance.CurrentPistolObject != null)
                return WeaponsManager.Instance.CurrentPistolObject;
            return dissolvePistol;
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _playerController = GetComponent<PlayerController>();
            if (weaponsManager == null) weaponsManager = GetComponent<WeaponsManager>();
            if (weaponsManager == null) weaponsManager = GetComponentInParent<WeaponsManager>();
            if (weaponsActions == null) weaponsActions = GetComponent<WeaponsActions>();
            if (weaponsActions == null) weaponsActions = GetComponentInParent<WeaponsActions>();
            if (weaponsActions == null) weaponsActions = GetComponentInChildren<WeaponsActions>();
            if (pistolItem == null) pistolItem = GetComponentInChildren<PistolItem>();
            if (meeleItem == null) meeleItem = GetComponentInChildren<MeeleItem>();
        }

        private void Start()
        {
            if (_animator != null)
            {
                _animator.SetBool(PlayerController.IsIdleBoolHash, true);
            }

            InitializeDissolveObjects();
        }

        public void InitializeWeaponDissolve(GameObject weaponObj, bool isActive = false)
        {
            if (weaponObj == null) return;
            float initialVal = isActive ? 1f : 0f;
            SetDissolveValue(weaponObj, initialVal);
            _currentDissolveValues[weaponObj] = initialVal;
            weaponObj.SetActive(isActive);
        }

        private void InitializeDissolveObjects()
        {
            GameObject activeMelee = GetActiveMeleeObject();
            if (activeMelee != null)
            {
                InitializeWeaponDissolve(activeMelee, false);
            }
            if (dissolvePedang != null && dissolvePedang != activeMelee)
            {
                InitializeWeaponDissolve(dissolvePedang, false);
            }

            GameObject activePistol = GetActivePistolObject();
            if (activePistol != null)
            {
                InitializeWeaponDissolve(activePistol, false);
            }
            if (dissolvePistol != null && dissolvePistol != activePistol)
            {
                InitializeWeaponDissolve(dissolvePistol, false);
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
            if (_playerController != null && _playerController.IsRolling) return;

            _isAttacking = true;
            GameObject activeMelee = GetActiveMeleeObject();
            TriggerDissolveIn(activeMelee);

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

            if (weaponsActions != null)
            {
                weaponsActions.StartMeleeAttack();
            }
            else if (WeaponsActions.Instance != null)
            {
                WeaponsActions.Instance.StartMeleeAttack();
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
            if (_playerController != null && _playerController.IsRolling) return;

            _isShooting = true;
            GameObject activePistol = GetActivePistolObject();
            TriggerDissolveIn(activePistol);

            _animator.SetBool(PlayerController.IsIdleBoolHash, false);

            _animator.ResetTrigger(PlayerController.ShootTriggerHash);
            _animator.SetTrigger(PlayerController.ShootTriggerHash);

            WeaponsActions wa = weaponsActions != null ? weaponsActions : WeaponsActions.Instance;
            _currentTargetEnemy = wa != null ? wa.GetTargetInFov(transform) : null;

            if (_currentTargetEnemy != null)
            {
                RotateTowardsEnemy(_currentTargetEnemy);
            }

            if (_shootResetCoroutine != null)
            {
                StopCoroutine(_shootResetCoroutine);
            }

            float delay = wa != null ? wa.shootDelay : 0.5f;
            float duration = wa != null ? wa.shootDuration : 0.4f;

            _shootResetCoroutine = StartCoroutine(PerformShootRoutine(delay, duration));
        }

        private void RotateTowardsEnemy(Transform target)
        {
            if (target == null) return;
            Vector3 dir = target.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dir.normalized);
            }
        }

        private IEnumerator PerformShootRoutine(float delay, float duration)
        {
            float elapsed = 0f;
            while (elapsed < delay)
            {
                elapsed += Time.deltaTime;
                if (_currentTargetEnemy != null)
                {
                    RotateTowardsEnemy(_currentTargetEnemy);
                }
                yield return null;
            }

            if (_currentTargetEnemy != null)
            {
                RotateTowardsEnemy(_currentTargetEnemy);
            }

            FireBullet(_currentTargetEnemy);

            // Durasi setelah menembak sebelum kembali ke idle
            if (duration > 0f)
            {
                yield return new WaitForSeconds(duration);
            }

            OnShootComplete();
        }

        public void FireBullet(Transform targetEnemy = null)
        {
            if (weaponsActions != null)
            {
                weaponsActions.Shoot(targetEnemy);
            }
            else if (WeaponsActions.Instance != null)
            {
                WeaponsActions.Instance.Shoot(targetEnemy);
            }
        }

        public void OnShootComplete()
        {
            _isShooting = false;
            _currentTargetEnemy = null;
            GameObject activePistol = GetActivePistolObject();

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
                TriggerDissolveOut(activePistol);
            }
            else
            {
                StartDissolveOutSingle(activePistol);
            }
        }


        public void OnAttackComplete()
        {
            _isAttacking = false;
            GameObject activeMelee = GetActiveMeleeObject();

            if (weaponsActions != null)
            {
                weaponsActions.EndMeleeAttack();
            }
            else if (WeaponsActions.Instance != null)
            {
                WeaponsActions.Instance.EndMeleeAttack();
            }

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
                TriggerDissolveOut(activeMelee);
            }
            else
            {
                StartDissolveOutSingle(activeMelee);
            }
        }

        public void CancelAttackAndShoot()
        {
            _isAttacking = false;
            _isShooting = false;
            _currentTargetEnemy = null;

            if (weaponsActions != null)
            {
                weaponsActions.EndMeleeAttack();
            }
            else if (WeaponsActions.Instance != null)
            {
                WeaponsActions.Instance.EndMeleeAttack();
            }

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

            GameObject activeMelee = GetActiveMeleeObject();
            GameObject activePistol = GetActivePistolObject();

            // Sembunyikan senjata lawan secara langsung agar tidak pernah muncul bersamaan
            if (targetWeapon == activeMelee || targetWeapon == dissolvePedang)
            {
                ForceHideWeapon(activePistol);
                if (dissolvePistol != null && dissolvePistol != activePistol) ForceHideWeapon(dissolvePistol);

                if (activeMelee != null)
                {
                    StartDissolveInSingle(activeMelee);
                }
            }
            else if (targetWeapon == activePistol || targetWeapon == dissolvePistol)
            {
                ForceHideWeapon(activeMelee);
                if (dissolvePedang != null && dissolvePedang != activeMelee) ForceHideWeapon(dissolvePedang);

                if (activePistol != null)
                {
                    StartDissolveInSingle(activePistol);
                }
            }

            // Memunculkan dissolveObjects lain jika ada (kecuali pedang & pistol)
            if (dissolveObjects != null)
            {
                foreach (var obj in dissolveObjects)
                {
                    if (obj == null) continue;
                    if (obj == activeMelee || obj == activePistol || obj == dissolvePedang || obj == dissolvePistol) continue;

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
                    if (obj == GetActiveMeleeObject() || obj == GetActivePistolObject() || obj == dissolvePedang || obj == dissolvePistol) continue;

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