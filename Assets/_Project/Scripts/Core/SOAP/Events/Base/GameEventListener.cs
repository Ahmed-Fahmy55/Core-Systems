using UnityEngine;
using UnityEngine.Events;

namespace Zone8.SOAP.Events
{
    public class GameEventListener<T> : MonoBehaviour, IEventListener<T>
    {
        [SerializeField]
        private GameEvent<T> _gameEvent;

        [SerializeField]
        private UnityEvent<T> _response;

        private void OnEnable()
        {
            if (_gameEvent == null)
            {
                Logger.LogError($"[GameEventListener] No GameEvent assigned on '{name}'.", this);
                return;
            }

            _gameEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            if (_gameEvent != null)
                _gameEvent.UnregisterListener(this);
        }

        public void OnEventRaised(T item)
        {
            _response.Invoke(item);
        }
    }

    public class GameEventListener : GameEventListener<Unit>
    {
    }
}
