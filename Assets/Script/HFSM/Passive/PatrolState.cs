using HFSM.Core;
using HFSM.Combat;
using Enemy;
using UnityEngine;
using UnityEngine.AI;

namespace HFSM.Passive
{
    public class PatrolState : EnemyBaseState
    {
        public PatrolState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
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

            if (!brain.CanMove)
            {
                stateMachine.ChangeState(new IdleState(brain, stateMachine));
                return;
            }

            if (brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                return;
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                stateMachine.ChangeState(new IdleState(brain, stateMachine));
            }
        }

        public override void Exit()
        {
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
            NavMeshHit hit;

            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                brain.Agent.SetDestination(hit.position);
                return true;
            }

            return false;
        }
    }
}