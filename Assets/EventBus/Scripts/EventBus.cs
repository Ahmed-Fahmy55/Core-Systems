using System.Collections.Generic;
using UnityEngine;

namespace Bltzo.Events
{
    public static class EventBus<T> where T : IEvent
    {
        static readonly List<IEventBinding<T>> s_bindings = new List<IEventBinding<T>>();

        public static void Register(EventBinding<T> binding)
        {
            if (binding == null)
            {
                Debug.LogError("Binding is null");
                return;
            }

            if (s_bindings.Contains(binding))
            {
                return;
            }

            s_bindings.Add(binding);
        }

        public static void Deregister(EventBinding<T> binding)
        {
            if (!s_bindings.Contains(binding))
            {
                return;
            }
            s_bindings.Remove(binding);
        }

        public static void Raise(T eventData)
        {
            for (int i = s_bindings.Count - 1; i >= 0; i--)
            {
                if (s_bindings[i] == null)
                {
                    Debug.Log($"Removing null binding at index {i}");
                    s_bindings.RemoveAt(i);
                    continue;
                }
                s_bindings[i].OnEvent.Invoke(eventData);
            }
        }

        public static void Raise()
        {
            for (int i = s_bindings.Count - 1; i >= 0; i--)
            {
                if (s_bindings[i] == null)
                {
                    Debug.Log($"Removing null binding at index {i}");
                    s_bindings.RemoveAt(i);
                    continue;
                }
                s_bindings[i].OnEventNoArgs.Invoke();
            }
        }
        public static IReadOnlyList<IEventBinding<T>> GetBindings()
        {
            return s_bindings.AsReadOnly();
        }
        static void ClearBindings()
        {
            s_bindings.Clear();
        }
    }
}
