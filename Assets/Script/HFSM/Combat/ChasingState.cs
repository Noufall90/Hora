using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class ChasingState : EnemyBaseState
    {
        public ChasingState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        private float repathTimer;
        private const float RepathInterval = 0.1f;
        private Vector3 lastTargetPos = Vector3.positiveInfinity;

        public override void Enter()
        {
            base.Enter();
            repathTimer = 0f;
            lastTargetPos = Vector3.positiveInfinity;

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.PlayerTarget == null) return;

            repathTimer += Time.deltaTime;
            if (brain.HasActiveNavMeshAgent && (repathTimer >= RepathInterval || (brain.PlayerTarget.position - lastTargetPos).sqrMagnitude > 1.0f))
            {
                repathTimer = 0f;
                lastTargetPos = brain.PlayerTarget.position;
                brain.Agent.SetDestination(lastTargetPos);
            }

            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            if (!brain.CanMove || IsPlayerInDistance(effectiveAttackRange))
            {
                if (parentState is CombatState combatSuperState)
                {
                    combatSuperState.SetAttackSubState();
                }
                else
                {
                    ChangeSubState(brain.HFSMMeeleAttackState ?? (State)new MeeleAttackState(brain, stateMachine));
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