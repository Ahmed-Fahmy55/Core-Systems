using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SOAP.Events
{
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<IEventListener<T>> eventListeners = new();

        public void Raise(T item)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(item);
            }
        }

        public void RegisterListener(IEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(IEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }

    public struct Unit
    {
        public static Unit Default => Default;
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
