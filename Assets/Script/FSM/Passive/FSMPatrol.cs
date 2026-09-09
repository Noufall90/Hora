using Enemy;
using FSM.Core;
using FSM.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace FSM.Passive
{
    public class FSMPatrol : FSMEnemyBaseState
    {
        public FSMPatrol(EnemyBrain brain, FiniteStateMachine finiteStateMachine) 
            : base(brain, finiteStateMachine) { }

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
                finiteStateMachine.ChangeState(new FSMIdleState(brain, finiteStateMachine));
                return;
            }

            if (brain.IsPlayerDetected())
            {
                finiteStateMachine.ChangeState(new FSMMeeleState(brain, finiteStateMachine));
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