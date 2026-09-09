using Enemy;
using FSM.Core;
using FSM.Passive;
using UnityEngine;

namespace FSM.Combat
{
    public class FSMMeeleState : FSMEnemyBaseState
    {
        private IMeele meeleCapability;
        private float attackCooldown;
        private float cooldownTimer;

        public FSMMeeleState(EnemyBrain brain, FiniteStateMachine finiteStateMachine, float attackCooldown = 1.5f) 
            : base(brain, finiteStateMachine)
        {
            this.meeleCapability = brain as IMeele;
            this.attackCooldown = attackCooldown;
        }

        public override void Enter()
        {
            cooldownTimer = attackCooldown;
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
                finiteStateMachine.ChangeState(new FSMIdleState(brain, finiteStateMachine));
                return;
            }

            if (!brain.IsPlayerDetected())
            {
                finiteStateMachine.ChangeState(new FSMInvestigate(brain, finiteStateMachine, brain.LastKnownPlayerPosition));
                return;
            }

            brain.RotateTowardsPlayer();

            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            bool inRange = IsPlayerInDistance(effectiveAttackRange);

            if (inRange)
            {
                if (brain.HasActiveNavMeshAgent)
                {
                    brain.Agent.isStopped = true;
                }

                cooldownTimer += Time.deltaTime;
                if (cooldownTimer >= attackCooldown)
                {
                    meeleCapability?.MeeleAttack();
                    cooldownTimer = 0f;
                }
            }
            else
            {
                if (brain.HasActiveNavMeshAgent)
                {
                    brain.Agent.isStopped = false;
                    brain.Agent.SetDestination(brain.PlayerTarget.position);
                }
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (brain is EnemyMeele enemyMeele)
            {
                enemyMeele.StopAttack();
            }

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.ResetPath();
            }
        }
    }
}