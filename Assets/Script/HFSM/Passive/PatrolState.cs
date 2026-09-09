using HFSM.Core;
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

            if (!brain.CanMove)
            {
                ChangeSubState(new IdleState(brain, stateMachine));
                return;
            }

            if (brain.HasActiveNavMeshAgent && !brain.Agent.pathPending && brain.Agent.remainingDistance <= brain.Agent.stoppingDistance)
            {
                ChangeSubState(new IdleState(brain, stateMachine));
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