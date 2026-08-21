using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlayerWeapons
{
    public class WeaponsActions : MonoBehaviour
    {
        public static WeaponsActions Instance { get; private set; }

        [Header("Pistol")]
        public Transform firePoint;
        public GameObject bulletPrefab;

        [Header("Pistol Energy & UI")]
        [SerializeField] private Image pistolBar;
        private float _currentPistolEnergy = -1f;
        private float _maxPistolEnergy = 10f;

        public Image PistolBar { get => pistolBar; set { pistolBar = value; UpdatePistolBarUI(); } }
        public float CurrentPistolEnergy => _currentPistolEnergy;
        public float MaxPistolEnergy => _maxPistolEnergy;

        [Header("Pistol - Auto Aim / Target Lock")]
        public bool enableAutoAim = true;
        [Range(0f, 180f)] public float autoAimFov = 90f;
        public float autoAimMaxDistance = 15f;

        [Header("Meele")]
        [SerializeField] private Collider meleeCollider;

        [Header("References")]
        [SerializeField] private WeaponsManager weaponsManager;

        private bool _isMeleeAttacking = false;
        private float _currentMeleeDamage = 10f;
        private readonly HashSet<Health> _hitEnemies = new HashSet<Health>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }


            if (meleeCollider == null) meleeCollider = GetComponent<Collider>();
            if (weaponsManager == null) weaponsManager = GetComponent<WeaponsManager>();
            if (weaponsManager == null) weaponsManager = GetComponentInParent<WeaponsManager>();
            if (weaponsManager == null) weaponsManager = GetComponentInChildren<WeaponsManager>();
        }

        private void Start()
        {
            SetupMeleeCollider();
            SyncPistolEnergyWithData();
        }

        private void Update()
        {
            RechargePistolEnergy();
        }

        public void SyncPistolEnergyWithData()
        {
            WeaponsManager manager = GetWeaponsManager();
            if (manager != null)
            {
                PistolData data = manager.CurrentPistolData;
                float mag = data.magazineEnergy > 0 ? data.magazineEnergy : 10f;
                if (_currentPistolEnergy < 0f)
                {
                    _maxPistolEnergy = mag;
                    _currentPistolEnergy = mag;
                    UpdatePistolBarUI();
                }
                else if (Mathf.Abs(_maxPistolEnergy - mag) > 0.01f)
                {
                    _maxPistolEnergy = mag;
                    _currentPistolEnergy = Mathf.Min(_currentPistolEnergy, mag);
                    UpdatePistolBarUI();
                }
            }
        }

        public float GetActivePistolRechargeRate()
        {
            WeaponsManager manager = GetWeaponsManager();
            if (manager != null)
            {
                PistolData data = manager.CurrentPistolData;
                if (data.pistolRechargeRate > 0)
                {
                    return data.pistolRechargeRate;
                }
            }
            return 3f;
        }

        private void RechargePistolEnergy()
        {
            SyncPistolEnergyWithData();
            if (_currentPistolEnergy < _maxPistolEnergy)
            {
                float rechargeRate = GetActivePistolRechargeRate();
                _currentPistolEnergy = Mathf.Min(_maxPistolEnergy, _currentPistolEnergy + rechargeRate * Time.deltaTime);
                UpdatePistolBarUI();
            }
        }

        public bool CanShoot()
        {
            SyncPistolEnergyWithData();
            return _currentPistolEnergy >= 1f;
        }

        public void ConsumeShootEnergy()
        {
            _currentPistolEnergy = Mathf.Max(0f, _currentPistolEnergy - 1f);
            UpdatePistolBarUI();
        }

        public void UpdatePistolBarUI()
        {
            if (pistolBar != null && _maxPistolEnergy > 0f)
            {
                pistolBar.fillAmount = Mathf.Clamp01(_currentPistolEnergy / _maxPistolEnergy);
            }
        }

        private WeaponsManager GetWeaponsManager()
        {
            if (weaponsManager != null) return weaponsManager;
            return WeaponsManager.Instance;
        }

        public Transform GetActiveFirePoint()
        {
            WeaponsManager manager = GetWeaponsManager();
            if (manager != null && manager.CurrentPistolObject != null)
            {
                Transform childFP = manager.CurrentPistolObject.transform.Find("FirePoint");
                if (childFP != null) return childFP;

                Transform[] children = manager.CurrentPistolObject.GetComponentsInChildren<Transform>();
                foreach (var t in children)
                {
                    if (t.name.ToLower().Contains("firepoint")) return t;
                }
            }

            if (firePoint != null) return firePoint;
            return transform;
        }

        public Transform GetTargetInFov(Transform originTransform = null)
        {
            if (!enableAutoAim) return null;

            Transform origin = originTransform != null ? originTransform : transform;
            Vector3 originPos = origin.position;
            Vector3 forward = origin.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Collider[] colliders = Physics.OverlapSphere(originPos, autoAimMaxDistance, LayerMask.GetMask("Enemy"));
            if (colliders == null || colliders.Length == 0) return null;

            Transform bestTarget = null;
            float bestScore = float.MaxValue;

            foreach (var col in colliders)
            {
                if (col == null) continue;

                if (col.CompareTag("Player") || col.transform.root == transform.root) continue;

                Health targetHealth = col.GetComponent<Health>() ?? col.GetComponentInParent<Health>();
                if (targetHealth == null || targetHealth.CurrentHealth <= 0) continue;

                Transform enemyTransform = targetHealth.transform;

                Vector3 dirToEnemy = enemyTransform.position - originPos;
                dirToEnemy.y = 0f;
                float distance = dirToEnemy.magnitude;

                if (distance < 0.1f || distance > autoAimMaxDistance) continue;

                dirToEnemy.Normalize();

                float angle = Vector3.Angle(forward, dirToEnemy);

                // Khusus jika musuh berada di depan player di dalam sudut FOV
                if (angle <= autoAimFov * 0.5f)
                {
                    // Prioritaskan musuh yang paling lurus di hadapan player
                    float score = angle + (distance * 2f);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestTarget = enemyTransform;
                    }
                }
            }

            return bestTarget;
        }

        public void Shoot(Transform targetEnemy = null)
        {
            if (!CanShoot()) return;
            ConsumeShootEnergy();

            GameObject prefabToSpawn = bulletPrefab;

            Transform spawnPoint = GetActiveFirePoint();
            if (spawnPoint == null)
            {
                return;
            }

            if (prefabToSpawn != null)
            {
                Transform target = targetEnemy != null ? targetEnemy : GetTargetInFov(transform);

                Vector3 fireDirection;
                if (target != null)
                {
                    fireDirection = target.position - spawnPoint.position;
                    fireDirection.y = 0f;
                }
                else
                {
                    fireDirection = spawnPoint.forward;
                    fireDirection.y = 0f;
                }

                if (fireDirection.sqrMagnitude < 0.001f)
                {
                    fireDirection = transform.forward;
                    fireDirection.y = 0f;
                }

                Quaternion straightRotation = Quaternion.LookRotation(fireDirection.normalized);
                Instantiate(prefabToSpawn, spawnPoint.position, straightRotation);
            }
        }

        #region Melee Actions & Trigger

        public void SetupMeleeCollider()
        {
            if (meleeCollider == null)
            {
                meleeCollider = GetComponent<Collider>();
            }

            // Jika belum ada collider, buatkan BoxCollider trigger di GameObject ini
            if (meleeCollider == null)
            {
                BoxCollider box = gameObject.AddComponent<BoxCollider>();
                box.size = new Vector3(1.2f, 1.8f, 1.5f);
                box.center = new Vector3(0f, 0.5f, 0.75f);
                meleeCollider = box;
            }

            if (meleeCollider != null)
            {
                meleeCollider.isTrigger = true;
                meleeCollider.enabled = false;
            }
        }

        public float GetActiveMeleeDamage()
        {
            WeaponsManager manager = GetWeaponsManager();
            if (manager != null)
            {
                MeeleData data = manager.CurrentMeleeData;
                return data.damage;
            }

            return 0f;
        }

        public void StartMeleeAttack(float damage = -1f)
        {
            _currentMeleeDamage = damage >= 0f ? damage : GetActiveMeleeDamage();
            _hitEnemies.Clear();
            _isMeleeAttacking = true;

            SetupMeleeCollider();

            if (meleeCollider != null)
            {
                meleeCollider.isTrigger = true;
                meleeCollider.enabled = true;
            }

            CheckOverlapDamage();
        }

        public void EndMeleeAttack()
        {
            _isMeleeAttacking = false;
            if (meleeCollider != null)
            {
                meleeCollider.enabled = false;
            }
            _hitEnemies.Clear();
        }

        private void CheckOverlapDamage()
        {
            if (!_isMeleeAttacking) return;

            Transform checkOrigin = meleeCollider != null ? meleeCollider.transform : transform;
            Vector3 center = checkOrigin.position + checkOrigin.forward * 0.8f;
            Collider[] overlaps = Physics.OverlapSphere(center, 1.5f, LayerMask.GetMask("Enemy"));

            foreach (var col in overlaps)
            {
                OnTriggerEnter(col);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isMeleeAttacking || other == null) return;

            if (other.CompareTag("Player") || other.transform.root == transform.root) return;

            int enemyLayerMask = LayerMask.GetMask("Enemy");
            bool isEnemyLayer = ((enemyLayerMask & (1 << other.gameObject.layer)) != 0) ||
                                ((enemyLayerMask & (1 << other.transform.root.gameObject.layer)) != 0);
            if (!isEnemyLayer) return;

            Health targetHealth = other.GetComponent<Health>() ?? other.GetComponentInParent<Health>();
            if (targetHealth != null && !_hitEnemies.Contains(targetHealth))
            {
                _hitEnemies.Add(targetHealth);
                targetHealth.TakeDamage((int)_currentMeleeDamage);
                Debug.Log("Attack damage");
            }
        }

        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (!enableAutoAim) return;

            Vector3 pos = transform.position + Vector3.up * 0.5f;

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Vector3 leftRay = Quaternion.Euler(0, -autoAimFov * 0.5f, 0) * forward * autoAimMaxDistance;
            Vector3 rightRay = Quaternion.Euler(0, autoAimFov * 0.5f, 0) * forward * autoAimMaxDistance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pos, pos + leftRay);
            Gizmos.DrawLine(pos, pos + rightRay);
            Gizmos.DrawLine(pos, pos + forward * autoAimMaxDistance);
        }
#endif
    }
}