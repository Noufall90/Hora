using Enemy;
using UnityEngine;

namespace HFSM.Core
{
    public abstract class EnemyBaseState : State
    {
        protected EnemyBrain brain;

        public EnemyBaseState(EnemyBrain brain, HierarchicalStateMachine stateMachine) : base(stateMachine)
        {
            this.brain = brain;
        }

        protected bool IsPlayerInDistance(float range)
        {
            if (brain == null || brain.PlayerTarget == null) return false;
            return Vector3.Distance(brain.transform.position, brain.PlayerTarget.position) <= range;
        }

        public void ChangeSubState(State newSubState)
        {
            if (parentState != null)
            {
                parentState.SetSubState(newSubState);
            }
            else
            {
                SetSubState(newSubState);
            }
        }
    }
}