using UnityEngine;
using HFSM.Core;

namespace Enemy
{
    [RequireComponent(typeof(EnemyHealth))]
    public abstract class EnemyBrain : MonoBehaviour
    {
        [Header("Base Movement Settings")]
        [SerializeField] protected float moveSpeed = 10f;
        
        [Header("Base Combat Settings")]
        [SerializeField] protected float attackDamage;
        [SerializeField] protected float detectRange;
        [SerializeField] protected float attackRange;

        [Header("HFSM Logic")]
        protected HierarchicalStateMachine hfsm;

        protected Transform playerTarget;
        protected EnemyHealth health;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float DetectRange => detectRange;
        public float AttackRange => attackRange;
        public Transform PlayerTarget => playerTarget;

        protected virtual void Awake()
        {
            health = GetComponent<EnemyHealth>();
        }

        protected virtual void Start()
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
            hfsm = new HierarchicalStateMachine();
            hfsm.Initialize(new HFSM.Passive.IdleState(this, hfsm));
        }

        protected virtual void Update()
        {
            hfsm.Update();
        }
        protected virtual void FixedUpdate()
        {
            hfsm?.FixedUpdate();
        }
    }
}