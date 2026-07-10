using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SOAP.Events
{
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<IEventListener<T>> _listeners = new();
        private IEventListener<T>[] _snapshot = Array.Empty<IEventListener<T>>();
        private bool _dirty;

        /// <summary>
        /// Raises the event over a copy-on-write snapshot, so listeners may register or
        /// unregister from inside their handler without corrupting the raise. A listener
        /// unregistered mid-raise is not invoked.
        /// </summary>
        public void Raise(T item)
        {
            if (_dirty)
            {
                _snapshot = _listeners.ToArray();
                _dirty = false;
            }

            foreach (var listener in _snapshot)
            {
                if (_listeners.Contains(listener))
                    listener.OnEventRaised(item);
            }
        }

        public void RegisterListener(IEventListener<T> listener)
        {
            if (listener == null || _listeners.Contains(listener)) return;

            _listeners.Add(listener);
            _dirty = true;
        }

        public void UnregisterListener(IEventListener<T> listener)
        {
            if (_listeners.Remove(listener))
                _dirty = true;
        }
    }

    public readonly struct Unit
    {
        public static Unit Default => default;
    }

    [CreateAssetMenu(menuName = "SOAP/Events/Game Event")]
    public class GameEvent : GameEvent<Unit>
    {
        public void Raise()
        {
            Raise(Unit.Default);
        }
    }
}
