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

        public void SetInitialInvestigatePos(Vector3? pos)
        {
            this.initialInvestigatePos = pos;
        }

        public override void Enter()
        {
            base.Enter();

            if (initialInvestigatePos.HasValue)
            {
                if (brain.HFSMInvestigateState != null)
                {
                    brain.HFSMInvestigateState.SetInvestigatePosition(initialInvestigatePos.Value);
                    SetSubState(brain.HFSMInvestigateState);
                }
                else
                {
                    SetSubState(new InvestigateState(brain, stateMachine, initialInvestigatePos.Value));
                }
                initialInvestigatePos = null;
            }
            else
            {
                SetSubState(brain.HFSMIdleState ?? (State)new IdleState(brain, stateMachine));
            }
        }

        public override void Update()
        {
            if (brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(brain.HFSMCombatState ?? (State)new CombatState(brain, stateMachine));
                return;
            }

            base.Update();
        }
    }
}
