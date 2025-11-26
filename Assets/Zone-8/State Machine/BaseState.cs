
namespace Zone8.StateMachine
{
    public abstract class BaseState
    {

        protected StateMachine _context;


        public BaseState(StateMachine stateMachine)
        {
            _context = stateMachine;
        }

        public abstract void OnStateEnter();

        public abstract void OnStateStay();

        public abstract void OnStateExit();

    }
}
