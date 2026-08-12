using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerWeapons
{
    public class WeaponsActions : MonoBehaviour
    {
        public static WeaponsActions Instance { get; private set; }

        [Header("Pistol")]
        public Transform firePoint;
        public GameObject bulletPrefab;
        public float shootDuration = 0.4f;
        public float shootDelay = 0.5f;

        [Header("Pistol - Auto Aim / Target Lock")]
        public bool enableAutoAim = true;
        [Range(0f, 180f)] public float autoAimFov = 90f;
        public float autoAimMaxDistance = 15f;

        [Header("Meele")]
        [SerializeField] private Collider meleeCollider;
        public float meleeDamage = 10f;
        public LayerMask enemyLayer;

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

            if (enemyLayer.value == 0)
            {
                enemyLayer = LayerMask.GetMask("Enemy");
            }

            if (meleeCollider == null) meleeCollider = GetComponent<Collider>();
            if (weaponsManager == null) weaponsManager = GetComponent<WeaponsManager>();
            if (weaponsManager == null) weaponsManager = GetComponentInParent<WeaponsManager>();
            if (weaponsManager == null) weaponsManager = GetComponentInChildren<WeaponsManager>();
        }

        private void Start()
        {
            SetupMeleeCollider();
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

            Collider[] colliders = Physics.OverlapSphere(originPos, autoAimMaxDistance, enemyLayer);
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

        public void StartMeleeAttack(float damage = -1f)
        {
            _currentMeleeDamage = damage >= 0f ? damage : meleeDamage;
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
            Collider[] overlaps = Physics.OverlapSphere(center, 1.5f, enemyLayer);

            foreach (var col in overlaps)
            {
                OnTriggerEnter(col);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isMeleeAttacking || other == null) return;

            if (other.CompareTag("Player") || other.transform.root == transform.root) return;

            bool isEnemyLayer = ((enemyLayer.value & (1 << other.gameObject.layer)) != 0) ||
                                ((enemyLayer.value & (1 << other.transform.root.gameObject.layer)) != 0);
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