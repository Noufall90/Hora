using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class IdleState : EnemyBaseState
    {
        private float idleTimer;
        private float idleDuration;

        public IdleState(EnemyBrain brain, HierarchicalStateMachine stateMachine, float idleDuration = 1.0f) 
            : base(brain, stateMachine)
        {
            this.idleDuration = idleDuration;
        }

        public override void Enter()
        {
            base.Enter();
            idleTimer = 0f;
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!brain.CanMove) return;

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                ChangeSubState(new PatrolState(brain, stateMachine));
            }
        }
    }
}