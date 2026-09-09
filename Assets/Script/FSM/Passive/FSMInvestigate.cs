using Enemy;
using FSM.Core;
using FSM.Combat;
using UnityEngine;

namespace FSM.Passive
{
    public class FSMInvestigate : FSMEnemyBaseState
    {
        private Vector3 targetPosition;

        public FSMInvestigate(EnemyBrain brain, FiniteStateMachine finiteStateMachine, Vector3 targetPosition) 
            : base(brain, finiteStateMachine)
        {
            this.targetPosition = targetPosition;
        }

        public override void Enter()
        {
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
                finiteStateMachine.ChangeState(new FSMMeeleState(brain, finiteStateMachine));
                return;
            }

            brain.RotateTowardsTarget(targetPosition);

            if (!brain.CanMove)
            {
                finiteStateMachine.ChangeState(new FSMIdleState(brain, finiteStateMachine));
                return;
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                finiteStateMachine.ChangeState(new FSMIdleState(brain, finiteStateMachine));
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