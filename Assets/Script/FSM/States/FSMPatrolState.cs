using Enemy;
using FSM.Core;
using UnityEngine;
using UnityEngine.AI;

namespace FSM.States
{
    public class FSMPatrolState : FSMState
    {
        public FSMPatrolState(EnemyBrain brain, FiniteStateMachine stateMachine) 
            : base(brain, stateMachine) { }

        public override void Enter()
        {
            base.Enter();

            if (!brain.CanMove) return;

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
                TrySetNewDestination();
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

        private bool TrySetNewDestination()
        {
            if (!brain.HasActiveNavMeshAgent) return false;

            Vector3 centre = brain.PatrolCentrePoint.position;
            Vector3 randomPoint = centre + Random.insideUnitSphere * brain.PatrolRange;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                brain.Agent.SetDestination(hit.position);
                return true;
            }

            return false;
        }
    }
}
