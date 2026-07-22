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
        [SerializeField] [Range(0f, 180f)] protected float fieldOfView = 60f; // dibatasi 0-180

        [Header("Debug")]
        [SerializeField] protected bool showDebugGizmos = true;

        [Header("HFSM Logic")]
        protected HierarchicalStateMachine hfsm;

        protected NavMeshAgent agent;
        protected Transform playerTarget;
        protected EnemyHealth health;

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
        public float PatrolRange => patrolRange;
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
        public float FieldOfView => fieldOfView;
        public NavMeshAgent Agent => agent;
        public Transform PlayerTarget => playerTarget;
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
            }
            hfsm = new HierarchicalStateMachine();
            hfsm.Initialize(new HFSM.Passive.IdleState(this, hfsm));
        }

        public bool IsPlayerDetected()
        {
            var proceduralAnimator = GetComponentInChildren<procedural_animation.EnemyProceduralAnimator>();
            if (proceduralAnimator != null)
            {
                // Jika ingin menggabungkan, bisa tambahkan logika di sini
            }

            if (playerTarget == null) return false;

            float distance = Vector3.Distance(transform.position, playerTarget.position);
            if (distance > detectRange) return false;

            return IsPlayerInViewCone(detectRange);
        }

        protected virtual bool IsPlayerInViewCone(float maxDistance)
        {
            if (playerTarget == null) return false;

            Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
            
            // Pastikan pemain berada di depan (dot > 0)
            float dot = Vector3.Dot(transform.forward, directionToPlayer);
            if (dot <= 0f) return false;

            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            return angle <= fieldOfView * 0.5f;
        }

        public bool IsPlayerInAttackRange()
        {
            if (playerTarget == null) return false;
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            return distance <= attackRange && IsPlayerInViewCone(attackRange);
        }

        protected virtual void Update()
        {
            if (HasActiveNavMeshAgent) agent.speed = moveSpeed;
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