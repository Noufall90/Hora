using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class ShooterAttackState : EnemyBaseState
    {
        private IShooter shooterCapability;
        private float cooldownTimer;

        public ShooterAttackState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine)
        {
            shooterCapability = brain as IShooter;
        }

        public override void Enter()
        {
            base.Enter();
            cooldownTimer = 0f;
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (!IsPlayerInDistance(brain.AttackRange))
            {
                if (brain.CanMove)
                {
                    ChangeSubState(new ChasingState(brain, stateMachine));
                    return;
                }
            }

            if (brain is EnemyMeeleShooter meeleShooterCheck && meeleShooterCheck.CurrentMode == EnemyMeeleShooter.MeeleShooterMode.Meele)
            {
                ChangeSubState(new MeeleAttackState(brain, stateMachine));
                return;
            }

            cooldownTimer += Time.deltaTime;
            if (shooterCapability != null && cooldownTimer >= shooterCapability.FireRate)
            {
                shooterCapability.ShootAttack();
                cooldownTimer = 0f;
            }
        }
    }
}