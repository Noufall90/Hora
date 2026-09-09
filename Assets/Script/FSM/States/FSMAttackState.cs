using Enemy;
using FSM.Core;
using UnityEngine;

namespace FSM.States
{
    public class FSMAttackState : FSMState
    {
        private IMeele meeleCapability;
        private float attackCooldown;
        private float cooldownTimer;

        public FSMAttackState(EnemyBrain brain, FiniteStateMachine stateMachine, float attackCooldown = 1.5f) 
            : base(brain, stateMachine)
        {
            this.meeleCapability = brain as IMeele;
            this.attackCooldown = attackCooldown;
        }

        public override void Enter()
        {
            base.Enter();
            cooldownTimer = attackCooldown;
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
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

            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            if (!IsPlayerInDistance(effectiveAttackRange))
            {
                if (brain.CanMove)
                {
                    stateMachine.ChangeState(new FSMChaseState(brain, stateMachine));
                    return;
                }
            }

            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= attackCooldown)
            {
                meeleCapability?.MeeleAttack();
                cooldownTimer = 0f;
            }
        }

        public override void Exit()
        {
            base.Exit();

            if (brain is EnemyMeele enemyMeele)
            {
                enemyMeele.StopAttack();
            }
            else if (brain is EnemyMeeleShooter enemyMeeleShooter)
            {
                enemyMeeleShooter.StopAttack();
            }
        }
    }
}
