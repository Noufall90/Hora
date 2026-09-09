using Enemy;
using UnityEngine;

namespace FSM.Core
{
    public abstract class FSMEnemyBaseState : FSMState
    {
        protected EnemyBrain brain;

        public FSMEnemyBaseState(EnemyBrain brain, FiniteStateMachine finiteStateMachine) : base(finiteStateMachine)
        {
            this.brain = brain;
        }

        protected bool IsPlayerInDistance(float range)
        {
            if (brain == null || brain.PlayerTarget == null) return false;
            return Vector3.Distance(brain.transform.position, brain.PlayerTarget.position) <= range;
        }
    }
}