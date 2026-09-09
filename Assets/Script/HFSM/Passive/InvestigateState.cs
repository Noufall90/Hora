using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class InvestigateState : EnemyBaseState
    {
        private Vector3 lastKnownPosition;

        public InvestigateState(EnemyBrain brain, HierarchicalStateMachine stateMachine, Vector3 lastKnownPosition) 
            : base(brain, stateMachine)
        {
            this.lastKnownPosition = lastKnownPosition;
        }

        public override void Enter()
        {
            base.Enter();

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                brain.Agent.SetDestination(lastKnownPosition);
            }
        }

        public override void Update()
        {
            base.Update();

            brain.RotateTowardsTarget(lastKnownPosition);

            if (!brain.CanMove)
            {
                ChangeSubState(new IdleState(brain, stateMachine));
                return;
            }

            if (brain.HasActiveNavMeshAgent)
            {
                if (!brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
                {
                    ChangeSubState(new IdleState(brain, stateMachine));
                }
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
