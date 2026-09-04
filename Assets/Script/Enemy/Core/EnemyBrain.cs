using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using HFSM.Core;

namespace Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyBrain : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] protected float moveSpeed;
        [SerializeField] protected float rotationSpeed = 50f;

        [Header("Base Patrol Settings")]
        [SerializeField] protected float patrolRange = 10f;
        [SerializeField] protected Transform patrolCentrePoint;

        [Header("Base Combat Settings")]
        [SerializeField] protected float detectRange;
        [SerializeField] protected float attackRange;
        [SerializeField] protected float meeleRange;
        [SerializeField] [Range(0f, 180f)] protected float fieldOfView = 60f; // dibatasi 0-180
        [SerializeField] protected LayerMask obstacleLayer;

        [Header("Knockback Settings")]
        [SerializeField] protected bool enableKnockback = true;
        [SerializeField] protected int requiredComboHits = 3;
        [SerializeField] protected float comboResetTime = 1.5f;
        [SerializeField] protected float knockbackForce = 15f;
        [SerializeField] protected float knockbackDuration = 0.3f;

        [Header("Debug")]
        [SerializeField] protected bool showDebugGizmos = true;

        [Header("HFSM Logic")]
        protected HierarchicalStateMachine hfsm;

        protected NavMeshAgent agent;
        protected Transform playerTarget;
        protected EnemyHealth health;
        protected Vector3 lastKnownPlayerPosition;

        protected int currentComboHits = 0;
        protected float comboResetTimer = 0f;
        protected bool isKnockedBack = false;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float PatrolRange => patrolRange;
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
        public float MeeleRange => meeleRange;
        public float FieldOfView => fieldOfView;
        public LayerMask ObstacleLayer => obstacleLayer;
        public NavMeshAgent Agent => agent;
        public Transform PlayerTarget => playerTarget;
        public State CurrentState => hfsm?.CurrentState;
        public bool IsInvestigating => hfsm?.CurrentState is HFSM.Passive.InvestigateState;
        public bool IsKnockedBack => isKnockedBack;
        public Vector3 LastKnownPlayerPosition
        {
            get
            {
                if (lastKnownPlayerPosition == Vector3.zero && playerTarget != null)
                    return playerTarget.position;
                return lastKnownPlayerPosition;
            }
            set => lastKnownPlayerPosition = value;
        }
        public bool HasActiveNavMeshAgent => agent != null && agent.enabled && agent.isOnNavMesh;
        public bool CanMove => moveSpeed > 0f && HasActiveNavMeshAgent;

        public virtual void RotateTowardsTarget(Vector3 targetPosition)
        {
            Vector3 direction = targetPosition - transform.position;
            direction.y = 0f;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public virtual void RotateTowardsPlayer()
        {
            if (playerTarget != null)
            {
                RotateTowardsTarget(playerTarget.position);
            }
        }

        public Transform PatrolCentrePoint => patrolCentrePoint != null ? patrolCentrePoint : transform;

        protected virtual void Awake()
        {
            health = GetComponent<EnemyHealth>();
            agent  = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            if (HasActiveNavMeshAgent)
                agent.speed = moveSpeed;

            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
                lastKnownPlayerPosition = player.transform.position;
            }

            if (health != null)
            {
                health.OnDamageTaken += OnDamageTakenHandler;
            }

            hfsm = new HierarchicalStateMachine();
            hfsm.Initialize(new HFSM.Passive.IdleState(this, hfsm));
        }

        protected virtual void OnDestroy()
        {
            if (health != null)
            {
                health.OnDamageTaken -= OnDamageTakenHandler;
            }
        }

        protected virtual void OnDamageTakenHandler(int damageAmount)
        {
            if (playerTarget == null) return;

            lastKnownPlayerPosition = playerTarget.position;

            Vector3 dirToAttacker = (playerTarget.position - transform.position);
            dirToAttacker.y = 0f;
            if (dirToAttacker.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(dirToAttacker.normalized);
            }

            var proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>() ?? GetComponent<procedural_animation.EnemyProceduralAnimator>();
            if (proceduralAnimator != null)
            {
                proceduralAnimator.SetLookTarget(playerTarget);
            }

            if (hfsm != null && (hfsm.CurrentState is HFSM.Passive.IdleState || hfsm.CurrentState is HFSM.Passive.PatrolState))
            {
                if (IsPlayerDetected())
                {
                    hfsm.ChangeState(new HFSM.Combat.ChasingState(this, hfsm));
                }
                else
                {
                    hfsm.ChangeState(new HFSM.Passive.InvestigateState(this, hfsm, lastKnownPlayerPosition));
                }
            }

            if (enableKnockback)
            {
                bool isDeathHit = health != null && health.CurrentHealth <= 0;
                if (isDeathHit)
                {
                    if (CameraShake.Instance != null)
                    {
                        CameraShake.Instance.CameraShaked(5f, 1f);
                    }
                    ApplyKnockback(2f);
                }
                else
                {
                    currentComboHits++;
                    comboResetTimer = comboResetTime;

                    if (currentComboHits >= requiredComboHits)
                    {
                        currentComboHits = 0;
                        ApplyKnockback(1f);
                    }
                }
            }
        }

        public virtual void ApplyKnockback(float forceMultiplier = 1f)
        {
            if (isKnockedBack) return;

            Vector3 knockbackDir = Vector3.zero;
            if (playerTarget != null)
            {
                knockbackDir = (transform.position - playerTarget.position);
                knockbackDir.y = 0f;
            }

            if (knockbackDir.sqrMagnitude < 0.001f)
            {
                knockbackDir = -transform.forward;
                knockbackDir.y = 0f;
            }

            knockbackDir.Normalize();
            StartCoroutine(KnockbackRoutine(knockbackDir, forceMultiplier));
        }

        protected virtual IEnumerator KnockbackRoutine(Vector3 direction, float forceMultiplier = 1f)
        {
            isKnockedBack = true;
            float elapsed = 0f;

            Animator anim = GetComponentInChildren<Animator>() ?? GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Hit");
            }

            float targetForce = knockbackForce * forceMultiplier;

            while (elapsed < knockbackDuration)
            {
                elapsed += Time.deltaTime;
                float currentForce = Mathf.Lerp(targetForce, 0f, elapsed / knockbackDuration);
                Vector3 moveStep = direction * currentForce * Time.deltaTime;

                if (HasActiveNavMeshAgent)
                {
                    agent.Move(moveStep);
                }
                else
                {
                    transform.position += moveStep;
                }

                yield return null;
            }

            isKnockedBack = false;
        }

        public bool IsPlayerDetected()
        {
            var proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>() ?? GetComponent<procedural_animation.EnemyProceduralAnimator>();
            bool detected = false;
            if (proceduralAnimator != null)
            {
                detected = proceduralAnimator.PlayerDetected;
            }
            else if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= detectRange)
            {
                detected = IsPlayerInViewCone(detectRange);
            }

            if (detected && playerTarget != null)
            {
                lastKnownPlayerPosition = playerTarget.position;
            }

            return detected;
        }

        protected virtual bool IsPlayerInViewCone(float maxDistance)
        {
            if (playerTarget == null) return false;

            Vector3 eyePos = transform.position + Vector3.up * 1f;
            Vector3 targetEyePos = playerTarget.position + Vector3.up * 1f;

            Vector3 directionToPlayer = (targetEyePos - eyePos).normalized;
            float distanceToPlayer = Vector3.Distance(eyePos, targetEyePos);

            if (distanceToPlayer > maxDistance) return false;

            float dot = Vector3.Dot(transform.forward, directionToPlayer);
            if (dot <= 0f) return false;

            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > fieldOfView * 0.5f) return false;

            if (obstacleLayer.value != 0 && Physics.Raycast(eyePos, directionToPlayer, distanceToPlayer, obstacleLayer))
            {
                return false;
            }

            return true;
        }

        public bool IsPlayerInAttackRange()
        {
            if (playerTarget == null) return false;
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            return distance <= attackRange && IsPlayerInViewCone(attackRange);
        }

        public bool IsPlayerInMeeleRange()
        {
            if (playerTarget == null) return false;
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            return distance <= meeleRange && IsPlayerInViewCone(meeleRange);
        }

        protected virtual void Update()
        {
            if (HasActiveNavMeshAgent) agent.speed = moveSpeed;

            if (enableKnockback && currentComboHits > 0)
            {
                comboResetTimer -= Time.deltaTime;
                if (comboResetTimer <= 0f)
                {
                    currentComboHits = 0;
                }
            }

            hfsm.Update();
        }

        protected virtual void FixedUpdate()
        {
            hfsm?.FixedUpdate();
        }

        protected virtual void OnDrawGizmosSelected()
        {
            if (!showDebugGizmos) return;

            Gizmos.color = Color.yellow;
            DrawViewCone(detectRange, Color.yellow);
            Gizmos.color = Color.red;
            DrawViewCone(attackRange, Color.red);
            if (meeleRange > 0f)
            {
                Gizmos.color = Color.cyan;
                DrawViewCone(meeleRange, Color.cyan);
            }
        }

        private void DrawViewCone(float range, Color color)
        {
            if (range <= 0f) return;

            Vector3 forward = transform.forward;
            Vector3 position = transform.position;

            float halfAngle = Mathf.Min(fieldOfView * 0.5f, 90f) * Mathf.Deg2Rad; // maks 90 derajat setengah
            int segments = 20;

            // Gambar busur kerucut (hanya di depan)
            for (int i = 0; i <= segments; i++)
            {
                float t = (float)i / segments;
                float angle = -halfAngle + t * 2f * halfAngle;
                Vector3 dir = Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f) * forward;
                Vector3 point = position + dir * range;

                if (i == 0)
                {
                    Gizmos.DrawLine(position, point);
                }
                else
                {
                    // gambar garis dari titik sebelumnya
                    float prevT = (float)(i - 1) / segments;
                    float prevAngle = -halfAngle + prevT * 2f * halfAngle;
                    Vector3 prevDir = Quaternion.Euler(0f, prevAngle * Mathf.Rad2Deg, 0f) * forward;
                    Vector3 prevPoint = position + prevDir * range;
                    Gizmos.DrawLine(prevPoint, point);
                }
            }

            // Garis tepi kiri dan kanan (sudah tergambar oleh loop di atas, tapi kita gambar ulang untuk kepastian)
            Vector3 leftDir = Quaternion.Euler(0f, -halfAngle * Mathf.Rad2Deg, 0f) * forward;
            Vector3 rightDir = Quaternion.Euler(0f, halfAngle * Mathf.Rad2Deg, 0f) * forward;
            Gizmos.DrawLine(position, position + leftDir * range);
            Gizmos.DrawLine(position, position + rightDir * range);

            // Garis tengah (hijau) untuk referensi
            Gizmos.color = Color.green;
            Gizmos.DrawLine(position, position + forward * range);
        }
    }
}