using Enemy;
using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public class FSMChaseState : FSMState
    {
        public FSMChaseState(EnemyBrain brain, FiniteStateMachine stateMachine) 
            : base(brain, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.PlayerTarget == null)
            {
                stateMachine.ChangeState(new FSMIdleState(brain, stateMachine));
                return;
            }

            if (!brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new FSMInvestigateState(brain, stateMachine, brain.LastKnownPlayerPosition));
                return;
            }

            brain.RotateTowardsPlayer();

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.SetDestination(brain.PlayerTarget.position);
            }

            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            if (!brain.CanMove || IsPlayerInDistance(effectiveAttackRange))
            {
                stateMachine.ChangeState(new FSMAttackState(brain, stateMachine));
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
