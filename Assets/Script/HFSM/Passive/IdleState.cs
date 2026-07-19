using HFSM.Core;
using HFSM.Combat;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class IdleState : EnemyBaseState
    {
        private float idleTimer;
        private float idleDuration = 0.5f;

        public IdleState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
            idleTimer = 0f;
            if (brain.Agent != null)
            {
                brain.Agent.isStopped = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                return;
            }

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                stateMachine.ChangeState(new PatrolState(brain, stateMachine));
            }
        }
    }
}