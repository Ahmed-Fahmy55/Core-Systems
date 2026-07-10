using UnityEngine;

namespace Zone8.StateMachine
{
    /// <summary>
    /// Minimal MonoBehaviour state machine. Derived classes provide the initial state;
    /// states drive transitions through <see cref="ChangeState"/>.
    /// </summary>
    public abstract class StateMachine : MonoBehaviour
    {
        public BaseState CurrentState { get; private set; }

        protected virtual void Start()
        {
            // A state set before Start (e.g. from another component's Awake) has already
            // been entered by ChangeState — entering it again here would double-enter it.
            if (CurrentState == null)
            {
                ChangeState(InitialState());
            }
        }

        protected virtual void Update()
        {
            CurrentState?.OnStateStay();
        }

        public void ChangeState(BaseState state)
        {
            if (state == null)
            {
                Logger.LogError($"[StateMachine] '{name}' cannot change to a null state.", this);
                return;
            }

            CurrentState?.OnStateExit();
            CurrentState = state;
            CurrentState.OnStateEnter();
        }

        /// <summary>The state the machine starts in when none was set before <see cref="Start"/>.</summary>
        protected abstract BaseState InitialState();
    }
}
