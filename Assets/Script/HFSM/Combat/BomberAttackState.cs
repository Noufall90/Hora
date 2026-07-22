using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class BomberAttackState : EnemyBaseState
    {
        private IBomber bomberCapability;
        private float cooldownTimer;

        public BomberAttackState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine)
        {
            bomberCapability = brain as IBomber;
        }

        public override void Enter()
        {
            cooldownTimer = (bomberCapability?.FireRate ?? 2.0f) * 0.5f; 
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

            brain.RotateTowardsPlayer();

            cooldownTimer += Time.deltaTime;
            float currentFireRate = bomberCapability?.FireRate ?? 2.0f;
            
            if (cooldownTimer >= currentFireRate)
            {
                bomberCapability?.ThrowGranade();
                cooldownTimer = 0f;
            }
        }
    }
}
