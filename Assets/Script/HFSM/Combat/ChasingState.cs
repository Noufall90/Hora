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
            if (brain.Agent != null)
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

            if (brain.Agent != null)
            {
                brain.Agent.SetDestination(brain.PlayerTarget.position);
            }

            brain.transform.LookAt(new Vector3(brain.PlayerTarget.position.x, brain.transform.position.y, brain.PlayerTarget.position.z));

            if (IsPlayerInDistance(brain.AttackRange))
            {
                if (brain is IMeele)
                {
                    stateMachine.ChangeState(new MeeleAttackState(brain, stateMachine));
                }
                else if (brain is IShooter)
                {
                    stateMachine.ChangeState(new ShooterAttackState(brain, stateMachine));
                }
            }
        }

        public override void Exit()
        {
            if (brain.Agent != null)
            {
                brain.Agent.ResetPath();
            }
        }
    }
}