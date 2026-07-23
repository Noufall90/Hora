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
            cooldownTimer = 0f;
        }

        public override void Update()
        {
            base.Update();

            if (!IsPlayerInDistance(brain.AttackRange))
            {
                if (brain.CanMove)
                {
                    stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                }
                return;
            }

            if (brain is EnemyMeeleShooter meeleShooterCheck && meeleShooterCheck.CurrentMode == EnemyMeeleShooter.MeeleShooterMode.Meele)
            {
                stateMachine.ChangeState(new MeeleAttackState(brain, stateMachine));
                return;
            }

            brain.RotateTowardsPlayer();

            cooldownTimer += Time.deltaTime;
            if (shooterCapability != null && cooldownTimer >= shooterCapability.FireRate)
            {
                shooterCapability.ShootAttack();
                cooldownTimer = 0f;
            }
        }
    }
}