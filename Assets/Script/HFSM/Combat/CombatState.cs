using Enemy;
using HFSM.Core;
using HFSM.Passive;
using UnityEngine;

namespace HFSM.Combat
{
    public class CombatState : EnemyBaseState
    {
        public CombatState(EnemyBrain brain, HierarchicalStateMachine stateMachine) 
            : base(brain, stateMachine) { }

        public override void Enter()
        {
            base.Enter();
            DecideInitialCombatSubState();
        }

        public override void Update()
        {
            if (brain.PlayerTarget == null)
            {
                stateMachine.ChangeState(new PassiveState(brain, stateMachine));
                return;
            }

            if (!brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new PassiveState(brain, stateMachine, brain.LastKnownPlayerPosition));
                return;
            }

            brain.RotateTowardsPlayer();

            base.Update();
        }

        public void DecideInitialCombatSubState()
        {
            float effectiveAttackRange = brain.AttackRange > 0 ? brain.AttackRange : brain.MeeleRange;
            bool inRange = IsPlayerInDistance(effectiveAttackRange);

            if (inRange || !brain.CanMove)
            {
                SetAttackSubState();
            }
            else
            {
                SetSubState(new ChasingState(brain, stateMachine));
            }
        }

        public void SetAttackSubState()
        {
            if (brain is EnemyMeeleShooter meeleShooter)
            {
                if (meeleShooter.CurrentMode == EnemyMeeleShooter.MeeleShooterMode.Meele)
                {
                    SetSubState(new MeeleAttackState(brain, stateMachine));
                }
                else
                {
                    SetSubState(new ShooterAttackState(brain, stateMachine));
                }
            }
            else if (brain is IMeele)
            {
                SetSubState(new MeeleAttackState(brain, stateMachine));
            }
            else if (brain is IShooter)
            {
                SetSubState(new ShooterAttackState(brain, stateMachine));
            }
            else if (brain is IBomber)
            {
                SetSubState(new BomberAttackState(brain, stateMachine));
            }
            else
            {
                SetSubState(new MeeleAttackState(brain, stateMachine));
            }
        }
    }
}
