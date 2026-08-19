using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerWeapons;

namespace PlayerData
{
    public class PlayerAnimAttack : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private float slashDuration = 0.4f;
        [SerializeField] private float shootDelay = 0.3f;

        [Header("Barang Dissolve")]
        [SerializeField] private GameObject dissolvePedang;
        [SerializeField] private GameObject dissolvePistol;
        [SerializeField] private float dissolveTimeBeforeUse = 0.5f;
        [SerializeField] private float dissolveTimeAfterUse = 0.5f;

        [Header("UI References")]
        [SerializeField] private UnityEngine.UI.Image pistolBar;

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

            WeaponsActions wa = weaponsActions != null ? weaponsActions : WeaponsActions.Instance;
            if (pistolBar != null && wa != null && wa.PistolBar == null)
            {
                wa.PistolBar = pistolBar;
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
            _comboResetCoroutine = StartCoroutine(ResetComboAfterDelay(slashDuration));
        }

        public void HandleShoot()
        {
            if (_animator == null) return;
            if (_playerController != null && _playerController.IsRolling) return;

            WeaponsActions wa = weaponsActions != null ? weaponsActions : WeaponsActions.Instance;
            if (wa != null && !wa.CanShoot()) return;

            bool wasShooting = _isShooting;
            _isShooting = true;

            GameObject activePistol = GetActivePistolObject();
            TriggerDissolveIn(activePistol);

            _animator.SetBool(PlayerController.IsIdleBoolHash, false);

            _animator.ResetTrigger(PlayerController.ShootTriggerHash);
            _animator.SetTrigger(PlayerController.ShootTriggerHash);

            _currentTargetEnemy = wa != null ? wa.GetTargetInFov(transform) : null;

            if (_currentTargetEnemy != null)
            {
                RotateTowardsEnemy(_currentTargetEnemy);
            }

            if (_shootResetCoroutine != null)
            {
                StopCoroutine(_shootResetCoroutine);
            }
            _shootResetCoroutine = StartCoroutine(ExecuteShootWithDelay(_currentTargetEnemy, !wasShooting));
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

        private IEnumerator ExecuteShootWithDelay(Transform targetEnemy, bool isInitialShot)
        {
            if (isInitialShot && shootDelay > 0f)
            {
                yield return new WaitForSeconds(shootDelay);
            }
            FireBullet(targetEnemy);
            OnShootComplete();
        }

        private IEnumerator ResetComboAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            _comboStep = 0;
            _comboResetCoroutine = null;
        }

        private void TriggerDissolveIn(GameObject targetWeapon = null)
        {
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
        }

        private void TriggerDissolveOut(GameObject targetWeapon = null)
        {
            if (targetWeapon != null)
            {
                StartDissolveOutSingle(targetWeapon);
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

        private void StartDissolveInSingle(GameObject obj, float? duration = null)
        {
            if (obj == null) return;
            float dur = duration ?? dissolveTimeBeforeUse;
            if (_activeDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            _activeDissolveCoroutines[obj] = StartCoroutine(AnimateDissolve(obj, 1f, dur, false));
        }

        private void StartDissolveOutSingle(GameObject obj, float? duration = null)
        {
            if (obj == null) return;
            float dur = duration ?? dissolveTimeAfterUse;
            if (_activeDissolveCoroutines.TryGetValue(obj, out Coroutine running) && running != null)
            {
                StopCoroutine(running);
            }
            _activeDissolveCoroutines[obj] = StartCoroutine(AnimateDissolve(obj, 0f, dur, true));
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