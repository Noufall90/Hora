using Enemy;
using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public class FSMIdleState : FSMState
    {
        private float idleTimer;
        private float idleDuration;

        public FSMIdleState(EnemyBrain brain, FiniteStateMachine stateMachine, float idleDuration = 1.0f) 
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

            if (brain.IsPlayerDetected())
            {
                float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
                if (IsPlayerInDistance(effectiveAttackRange))
                {
                    stateMachine.ChangeState(new FSMAttackState(brain, stateMachine));
                }
                else
                {
                    stateMachine.ChangeState(new FSMChaseState(brain, stateMachine));
                }
                return;
            }

            if (!brain.CanMove) return;

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                stateMachine.ChangeState(new FSMPatrolState(brain, stateMachine));
            }
        }
    }
}
