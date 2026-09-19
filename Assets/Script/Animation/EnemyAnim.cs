using UnityEngine;
using Enemy;

namespace EnemyAnimation
{
    public class EnemyAnim : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator _animator;
        [SerializeField] private EnemyMeele _meeleEnemy;

        [Header("Detection Settings (LOS)")]
        [SerializeField] private float _viewDistance = 15f;
        [Range(0f, 180f)]
        [SerializeField] private float _fovAngle = 100f;
        [SerializeField] private LayerMask _playerLayer;
        [SerializeField] private LayerMask _obstacleLayer;

        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmos = true;

        // Parameter Animator internal
        private static readonly int IsAttackTriggerHash = Animator.StringToHash("IsAttack");
        private static readonly int AttackBoolHash = Animator.StringToHash("Attack");
        private static readonly int IsIdleBoolHash = Animator.StringToHash("IsIdle");
        private static readonly int IsWalkBoolHash = Animator.StringToHash("IsWalk");
        private const float MoveSpeedThreshold = 0.1f;

        private bool _playerDetected;
        private bool _isAttacking;
        private bool _isMoving;

        private bool _hasIsAttackTrigger;
        private bool _hasAttackBool;
        private bool _hasIsIdleBool;
        private bool _hasIsWalkBool;

        public Animator Animator => _animator;
        public EnemyMeele MeeleEnemy => _meeleEnemy;
        public bool PlayerDetected => _playerDetected;
        public float ViewDistance => _viewDistance;
        public float FovAngle => _fovAngle;
        public bool IsMoving => _isMoving;
        public bool IsAttacking => _isAttacking;

        private void Awake()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
            }

            if (_meeleEnemy == null)
            {
                _meeleEnemy = GetComponentInParent<EnemyMeele>() ?? GetComponent<EnemyMeele>();
            }

            CacheAnimatorParameters();
        }

        private void Start()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>() ?? GetComponentInChildren<Animator>();
                CacheAnimatorParameters();
            }

            if (_meeleEnemy == null)
            {
                _meeleEnemy = GetComponentInParent<EnemyMeele>() ?? GetComponent<EnemyMeele>();
            }
        }

        private void CacheAnimatorParameters()
        {
            if (_animator == null) return;

            _hasIsAttackTrigger = HasParameter(_animator, IsAttackTriggerHash);
            _hasAttackBool = HasParameter(_animator, AttackBoolHash);
            _hasIsIdleBool = HasParameter(_animator, IsIdleBoolHash);
            _hasIsWalkBool = HasParameter(_animator, IsWalkBoolHash);
        }

        private static bool HasParameter(Animator anim, int paramHash)
        {
            if (anim == null) return false;
            foreach (var p in anim.parameters)
            {
                if (p.nameHash == paramHash) return true;
            }
            return false;
        }

        private void Update()
        {
            _CheckLineOfSight();
            UpdateMovementAnimation();
        }

        private void UpdateMovementAnimation()
        {
            if (_animator == null) return;

            bool moving = false;

            if (_meeleEnemy != null)
            {
                if (_meeleEnemy.HasActiveNavMeshAgent)
                {
                    moving = _meeleEnemy.Agent.velocity.sqrMagnitude > (MoveSpeedThreshold * MoveSpeedThreshold);
                }
                else
                {
                    moving = _meeleEnemy.MoveSpeed > 0f && !_meeleEnemy.IsKnockedBack;
                }
            }

            _isMoving = moving;

            if (_hasIsWalkBool)
            {
                _animator.SetBool(IsWalkBoolHash, moving);
            }

            if (_hasIsIdleBool)
            {
                _animator.SetBool(IsIdleBoolHash, !moving);
            }
        }

        private void _CheckLineOfSight()
        {
            Transform target = null;
            if (_meeleEnemy != null && _meeleEnemy.PlayerTarget != null)
            {
                target = _meeleEnemy.PlayerTarget;
            }
            else
            {
                GameObject player = GameObject.FindWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }

            if (target == null)
            {
                _playerDetected = false;
                return;
            }

            Vector3 eyePos = transform.position + Vector3.up * 1f;
            Vector3 targetEyePos = target.position + Vector3.up * 1f;

            float distance = Vector3.Distance(eyePos, targetEyePos);
            if (distance > _viewDistance)
            {
                _playerDetected = false;
                return;
            }

            // Hitung arah horizontal (XZ) murni untuk mencegah deteksi semu di belakang
            Vector3 horizontalDir = (targetEyePos - eyePos);
            horizontalDir.y = 0f;

            if (horizontalDir.sqrMagnitude < 0.001f)
            {
                _playerDetected = false;
                return;
            }

            horizontalDir.Normalize();

            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            float angle = Vector3.Angle(forward, horizontalDir);
            float halfFov = Mathf.Clamp(_fovAngle * 0.5f, 0f, 90f);

            // Jika berada di luar sudut pandang depan, abaikan target
            if (angle > halfFov)
            {
                _playerDetected = false;
                return;
            }

            // Cek rintangan/obstacle menggunakan Raycast
            Vector3 rayDir = (targetEyePos - eyePos).normalized;
            LayerMask obstacle = _obstacleLayer.value != 0 
                ? _obstacleLayer 
                : (_meeleEnemy != null ? _meeleEnemy.ObstacleLayer : default);

            if (obstacle.value != 0 && Physics.Raycast(eyePos, rayDir, distance, obstacle))
            {
                _playerDetected = false;
                return;
            }

            _playerDetected = true;
            if (_meeleEnemy != null)
            {
                _meeleEnemy.LastKnownPlayerPosition = target.position;
            }
        }

        public void TriggerAttack()
        {
            _isAttacking = true;

            if (_animator != null)
            {
                if (_hasIsAttackTrigger)
                {
                    _animator.SetTrigger(IsAttackTriggerHash);
                }
                if (_hasAttackBool)
                {
                    _animator.SetBool(AttackBoolHash, true);
                }
            }
        }

        public void StopAttack()
        {
            _isAttacking = false;

            if (_animator != null)
            {
                if (_hasIsAttackTrigger)
                {
                    _animator.ResetTrigger(IsAttackTriggerHash);
                }
                if (_hasAttackBool)
                {
                    _animator.SetBool(AttackBoolHash, false);
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!_showDebugGizmos) return;

            Vector3 eyePos = transform.position + Vector3.up * 1f;
            Vector3 forward = transform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();

            Gizmos.color = _playerDetected ? Color.green : Color.yellow;

            float halfAngle = Mathf.Clamp(_fovAngle * 0.5f, 0f, 90f);
            int segments = 24;

            // Gambar busur sektor pandangan di depan NPC (bukan sphere)
            Vector3 prevPoint = eyePos + (Quaternion.Euler(0f, -halfAngle, 0f) * forward * _viewDistance);
            Gizmos.DrawLine(eyePos, prevPoint);

            for (int i = 1; i <= segments; i++)
            {
                float t = (float)i / segments;
                float currentAngle = Mathf.Lerp(-halfAngle, halfAngle, t);
                Vector3 currentPoint = eyePos + (Quaternion.Euler(0f, currentAngle, 0f) * forward * _viewDistance);
                Gizmos.DrawLine(prevPoint, currentPoint);
                prevPoint = currentPoint;
            }

            Gizmos.DrawLine(eyePos, prevPoint);

            // Garis tengah hadapan
            Gizmos.color = _playerDetected ? Color.green : new Color(1f, 0.92f, 0.016f, 0.5f);
            Gizmos.DrawLine(eyePos, eyePos + forward * _viewDistance);
        }
    }
}