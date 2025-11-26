using UnityEngine;
namespace Zone8.StateMachine
{
    public abstract class StateMachine : MonoBehaviour
    {

        private BaseState _currentState;


        protected virtual void Start()
        {
            if (_currentState != null)
            {
                _currentState.OnStateEnter();
            }
            else
            {
                ChangeState(IntialStat());
            }
        }

        protected virtual void Update()
        {
            if (_currentState != null)
            {
                _currentState.OnStateStay();
            }

        }

        public void ChangeState(BaseState state)
        {
            _currentState?.OnStateExit();
            _currentState = state;
            _currentState.OnStateEnter();
        }

        public abstract BaseState IntialStat();
    }
}
