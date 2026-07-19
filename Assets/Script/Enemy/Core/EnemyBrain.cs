using UnityEngine;
using UnityEngine.AI;
using HFSM.Core;

namespace Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBrain : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] protected float moveSpeed;

        [Header("Base Patrol Settings")]
        [SerializeField] protected float patrolRange = 10f;
        [SerializeField] protected Transform patrolCentrePoint;

        [Header("Base Combat Settings")]
        [SerializeField] protected float attackDamage;
        [SerializeField] protected float detectRange;
        [SerializeField] protected float attackRange;

        [Header("HFSM Logic")]
        protected HierarchicalStateMachine hfsm;

        protected NavMeshAgent agent;
        protected Transform playerTarget;
        protected EnemyHealth health;

        public float MoveSpeed => moveSpeed;
        public float PatrolRange => patrolRange;
        public float AttackDamage => attackDamage;
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
        public NavMeshAgent Agent => agent;
        public Transform PlayerTarget => playerTarget;

        public Transform PatrolCentrePoint => patrolCentrePoint != null ? patrolCentrePoint : transform;

        protected virtual void Awake()
        {
            health = GetComponent<EnemyHealth>();
            agent  = GetComponent<NavMeshAgent>();
        }

        protected virtual void Start()
        {
            if (agent != null)
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
                return proceduralAnimator.PlayerDetected;
            }

            if (playerTarget == null) return false;
            return Vector3.Distance(transform.position, playerTarget.position) <= detectRange;
        }

        protected virtual void Update()
        {
            if (agent != null) agent.speed = moveSpeed;
            hfsm.Update();
        }

        protected virtual void FixedUpdate()
        {
            hfsm?.FixedUpdate();
        }
    }
}