using UnityEngine;

namespace HFSM.Core
{
    public abstract class State
    {
        protected HierarchicalStateMachine stateMachine;
        protected State parentState;
        protected State subState;

        public State(HierarchicalStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { subState?.Update(); }
        public virtual void FixedUpdate() { subState?.FixedUpdate(); }
        public virtual void Exit() { subState?.Exit(); }

        // Fungsi internal untuk mengatur sub-state (hierarki bertingkat)
        protected void SetSubState(State newSubState)
        {
            subState?.Exit();
            subState = newSubState;
            newSubState.parentState = this;
            newSubState.Enter();
        }
    }
}