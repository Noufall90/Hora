using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class MeeleAttackState : EnemyBaseState
    {
        private IMeele meeleCapability;
        private float attackCooldown = 1.5f;
        private float cooldownTimer;

        public MeeleAttackState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine)
        {
            meeleCapability = brain as IMeele;
        }

        public override void Enter()
        {
            cooldownTimer = attackCooldown;
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
        }
    }
}