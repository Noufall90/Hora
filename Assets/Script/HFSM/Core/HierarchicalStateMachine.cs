namespace HFSM.Core
{
    public class HierarchicalStateMachine
    {
        public State CurrentState { get; private set; }

        public void Initialize(State initialState)
        {
            CurrentState = initialState;
            CurrentState.Enter();
        }

        public void ChangeState(State newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState.Enter();
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }
    }
}