using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SOAP.Events
{
    public abstract class GameEvent<T> : ScriptableObject
    {
        private readonly List<IGameEventListener<T>> eventListeners = new();

        public void Raise(T item)
        {
            for (int i = eventListeners.Count - 1; i >= 0; i--)
            {
                eventListeners[i].OnEventRaised(item);
            }
        }

        public void RegisterListener(IGameEventListener<T> listener)
        {
            if (!eventListeners.Contains(listener))
                eventListeners.Add(listener);
        }

        public void UnregisterListener(IGameEventListener<T> listener)
        {
            if (eventListeners.Contains(listener))
                eventListeners.Remove(listener);
        }
    }

    public struct Unit
    {
        public static Unit Default => Default;
    }

    [CreateAssetMenu(menuName = "Zone8/SOAP/Events/Game Event")]
    public class GameEvent : GameEvent<Unit>
    {
        public void Raise()
        {
            Raise(Unit.Default);
        }
    }
}
