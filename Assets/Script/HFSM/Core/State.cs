using UnityEngine;

namespace HFSM.Core
{
    public abstract class State
    {
        protected HierarchicalStateMachine stateMachine;
        protected State parentState;
        protected State subState;

        public State CurrentSubState => subState;
        public State ParentState => parentState;

        public State(HierarchicalStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        public virtual void Enter() { }
        public virtual void Update() { subState?.Update(); }
        public virtual void FixedUpdate() { subState?.FixedUpdate(); }
        public virtual void Exit() { subState?.Exit(); }

        public void SetSubState(State newSubState)
        {
            subState?.Exit();
            subState = newSubState;
            if (newSubState != null)
            {
                newSubState.parentState = this;
                newSubState.Enter();
            }
        }
    }
}