using UnityEngine;
using UnityEngine.Events;

namespace Zone8.SOAP.Events
{
    public class GameEventListener<T> : MonoBehaviour, IGameEventListener<T>
    {
        [SerializeField]
        private GameEvent<T> gameEvent;

        [SerializeField]
        private UnityEvent<T> response;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(T item)
        {
            response.Invoke(item);
        }
    }

    public class GameEventListener : MonoBehaviour, IGameEventListener<Unit>
    {
        [SerializeField]
        private GameEvent gameEvent;
        [SerializeField]
        private UnityEvent response;

        private void OnEnable()
        {
            gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(Unit item)
        {
            response.Invoke();
        }
    }
}
