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
                if (!brain.CanMove)
                {
                    if (brain is IShooter)
                    {
                        stateMachine.ChangeState(new ShooterAttackState(brain, stateMachine));
                    }
                    else if (brain is IMeele)
                    {
                        stateMachine.ChangeState(new MeeleAttackState(brain, stateMachine));
                    }
                    else if (brain is IBomber)
                    {
                        stateMachine.ChangeState(new BomberAttackState(brain, stateMachine));
                    }
                    return;
                }

                stateMachine.ChangeState(new ChasingState(brain, stateMachine));
                return;
            }

            if (!brain.CanMove) return;

            idleTimer += Time.deltaTime;
            if (idleTimer >= idleDuration)
            {
                stateMachine.ChangeState(new PatrolState(brain, stateMachine));
            }
        }
    }
}