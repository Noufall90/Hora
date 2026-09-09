using Enemy;
using FSM.Core;
using FSM.Combat;
using UnityEngine;

namespace FSM.Passive
{
    public class FSMIdleState : FSMEnemyBaseState
    {
        private float idleTimer;
        private float idleDuration;

        public FSMIdleState(EnemyBrain brain, FiniteStateMachine finiteStateMachine, float idleDuration = 1.0f) 
            : base(brain, finiteStateMachine)
        {
            this.idleDuration = idleDuration;
        }

        public override void Enter()
        {
            idleTimer = 0f;
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = true;
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.IsPlayerDetected())
            {
                finiteStateMachine.ChangeState(new FSMMeeleState(brain, finiteStateMachine));
                return;
            }

            if (!brain.CanMove) return;

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                finiteStateMachine.ChangeState(new FSMPatrol(brain, finiteStateMachine));
            }
        }
    }
}