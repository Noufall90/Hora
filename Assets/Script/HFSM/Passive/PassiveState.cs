using Enemy;
using HFSM.Core;
using HFSM.Combat;
using UnityEngine;

namespace HFSM.Passive
{
    public class PassiveState : EnemyBaseState
    {
        private Vector3? initialInvestigatePos;

        public PassiveState(EnemyBrain brain, HierarchicalStateMachine stateMachine, Vector3? initialInvestigatePos = null) 
            : base(brain, stateMachine)
        {
            this.initialInvestigatePos = initialInvestigatePos;
        }

        public override void Enter()
        {
            base.Enter();

            if (initialInvestigatePos.HasValue)
            {
                SetSubState(new InvestigateState(brain, stateMachine, initialInvestigatePos.Value));
            }
            else
            {
                SetSubState(new IdleState(brain, stateMachine));
            }
        }

        public override void Update()
        {
            if (brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new CombatState(brain, stateMachine));
                return;
            }

            base.Update();
        }
    }
}
