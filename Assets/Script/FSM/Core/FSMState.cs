using Enemy;
using UnityEngine;

namespace FSM.Core
{
    public abstract class FSMState
    {
        protected EnemyBrain brain;
        protected FiniteStateMachine stateMachine;

        public FSMState(EnemyBrain brain, FiniteStateMachine stateMachine)
        {
            this.brain = brain;
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }

        protected bool IsPlayerInDistance(float range)
        {
            if (brain == null || brain.PlayerTarget == null) return false;
            return Vector3.Distance(brain.transform.position, brain.PlayerTarget.position) <= range;
        }
    }
}