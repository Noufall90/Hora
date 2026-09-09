using Enemy;
using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public class FSMInvestigateState : FSMState
    {
        private Vector3 targetPosition;

        public FSMInvestigateState(EnemyBrain brain, FiniteStateMachine stateMachine, Vector3 targetPosition) 
            : base(brain, stateMachine)
        {
            this.targetPosition = targetPosition;
        }

        public override void Enter()
        {
            base.Enter();

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                brain.Agent.SetDestination(targetPosition);
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

            brain.RotateTowardsTarget(targetPosition);

            if (!brain.CanMove)
            {
                stateMachine.ChangeState(new FSMIdleState(brain, stateMachine));
                return;
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                stateMachine.ChangeState(new FSMIdleState(brain, stateMachine));
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.ResetPath();
            }
        }
    }
}
