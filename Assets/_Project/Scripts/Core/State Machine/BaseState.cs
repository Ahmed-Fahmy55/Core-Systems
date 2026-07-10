namespace Zone8.StateMachine
{
    /// <summary>A single state owned by a <see cref="StateMachine"/>.</summary>
    public abstract class BaseState
    {
        protected readonly StateMachine _context;

        protected BaseState(StateMachine stateMachine)
        {
            _context = stateMachine;
        }

        public abstract void OnStateEnter();

        public abstract void OnStateStay();

        public abstract void OnStateExit();
    }
}
