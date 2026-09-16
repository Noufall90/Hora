using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class MeeleAttackState : EnemyBaseState
    {
        private IMeele meeleCapability;
        private float attackCooldown;
        private float cooldownTimer;

        public MeeleAttackState(EnemyBrain brain, HierarchicalStateMachine stateMachine, float attackCooldown = 1.5f) 
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

            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            if (!IsPlayerInDistance(effectiveAttackRange))
            {
                if (brain.CanMove)
                {
                    ChangeSubState(brain.HFSMChasingState ?? (State)new ChasingState(brain, stateMachine));
                    return;
                }
            }

            if (brain is EnemyMeeleShooter meeleShooterCheck && meeleShooterCheck.CurrentMode == EnemyMeeleShooter.MeeleShooterMode.Shooter)
            {
                ChangeSubState(brain.HFSMShooterAttackState ?? (State)new ShooterAttackState(brain, stateMachine));
                return;
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