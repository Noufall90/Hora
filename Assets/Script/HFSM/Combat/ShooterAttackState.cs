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
                // Jika player menjauh dan objeknya bukan Tower (bisa bergerak), kejar kembali
                if (brain.MoveSpeed > 0)
                {
                    stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                }
                return;
            }

            // Hadapkan badan/senjata ke arah target (dapat digabung dengan Look IK)
            if (brain.PlayerTarget != null)
            {
                brain.transform.LookAt(new Vector3(brain.PlayerTarget.position.x, brain.transform.position.y, brain.PlayerTarget.position.z));
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