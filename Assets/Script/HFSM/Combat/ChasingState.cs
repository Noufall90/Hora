using HFSM.Core;
using Enemy;
using UnityEngine;

namespace HFSM.Combat
{
    public class ChasingState : EnemyBaseState
    {
        public ChasingState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(brain, stateMachine) { }

        public override void Enter()
        {
            Debug.Log($"{brain.gameObject.name} mengejar target");
        }

        public override void Update()
        {
            base.Update();

            if (brain.PlayerTarget == null) return;

            brain.transform.position = Vector3.MoveTowards(brain.transform.position, brain.PlayerTarget.position, brain.MoveSpeed * Time.deltaTime);
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
    }
}