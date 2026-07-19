using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Passive
{
    public class IdleState : EnemyBaseState
    {
        private float idleTimer;
        private float idleDuration = 2f;

        public IdleState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
            idleTimer = 0f;
            Debug.Log($"{brain.gameObject.name} memasuki IdleState");
        }

        public override void Update()
        {
            base.Update();

            if (IsPlayerInDistance(brain.DetectRange))
            {
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