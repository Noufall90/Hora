using UnityEngine;

namespace FSM.Core
{
    public abstract class FSMState
    {
        protected FiniteStateMachine finiteStateMachine;

        public FSMState(FiniteStateMachine finiteStateMachine)
        {
            this.finiteStateMachine = finiteStateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void Exit() { }
    }
}