using HFSM.Core;
using HFSM.Passive;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class ChasingState : EnemyBaseState
    {
        public ChasingState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.isStopped = false;
                brain.Agent.speed = brain.MoveSpeed;
            }
        }

        public override void Update()
        {
            base.Update();

            if (brain.PlayerTarget == null) return;

            if (!brain.IsPlayerDetected())
            {
                stateMachine.ChangeState(new InvestigateState(brain, stateMachine, brain.PlayerTarget.position));
                return;
            }

            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.SetDestination(brain.PlayerTarget.position);
            }

            brain.RotateTowardsPlayer();

            if (!brain.CanMove || IsPlayerInDistance(brain.AttackRange))
            {
                if (brain is EnemyMeeleShooter meeleShooter)
                {
                    if (meeleShooter.CurrentMode == EnemyMeeleShooter.MeeleShooterMode.Meele)
                    {
                        stateMachine.ChangeState(new MeeleAttackState(brain, stateMachine));
                    }
                    else
                    {
                        stateMachine.ChangeState(new ShooterAttackState(brain, stateMachine));
                    }
                }
                else if (brain is IMeele)
                {
                    stateMachine.ChangeState(new MeeleAttackState(brain, stateMachine));
                }
                else if (brain is IShooter)
                {
                    stateMachine.ChangeState(new ShooterAttackState(brain, stateMachine));
                }
                else if (brain is IBomber)
                {
                    stateMachine.ChangeState(new BomberAttackState(brain, stateMachine));
                }
            }
        }

        public override void Exit()
        {
            if (brain.HasActiveNavMeshAgent)
            {
                brain.Agent.ResetPath();
            }
        }
    }
}