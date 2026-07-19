using HFSM.Core;
using HFSM.Combat;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class InvestigateState : EnemyBaseState
    {
        private Vector3 lastKnownPosition;

        public InvestigateState(EnemyBrain brain, HierarchicalStateMachine stateMachine, Vector3 lastKnownPosition) : base(brain, stateMachine)
        {
            this.lastKnownPosition = lastKnownPosition;
        }

        public override void Enter()
        {
            if (brain.Agent != null)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                brain.Agent.SetDestination(lastKnownPosition);
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                return;
            }

            if (brain.Agent != null)
            {
                if (!brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
                {
                    stateMachine.ChangeState(new IdleState(brain, stateMachine));
                }
            }
        }

        public override void Exit()
        {
            if (brain.Agent != null)
            {
                brain.Agent.ResetPath();
            }
        }
    }
}
