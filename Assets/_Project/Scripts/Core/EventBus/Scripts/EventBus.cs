using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.Events
{
    /// <summary>
    /// Static, type-safe event bus. Raising iterates a copy-on-write snapshot, so bindings
    /// may be registered/deregistered from inside a handler without corrupting the raise:
    /// a binding deregistered mid-raise is not invoked, one registered mid-raise is invoked
    /// starting from the next raise.
    /// </summary>
    public static class EventBus<T> where T : IEvent
    {
        static readonly HashSet<IEventBinding<T>> s_bindings = new();
        static IEventBinding<T>[] s_snapshot = Array.Empty<IEventBinding<T>>();
        static bool s_dirty;

        public static void Register(EventBinding<T> binding)
        {
            if (binding == null)
            {
                Debug.LogError($"[EventBus<{typeof(T).Name}>] Cannot register a null binding.");
                return;
            }

            if (s_bindings.Add(binding))
                s_dirty = true;
        }

        public static void Deregister(EventBinding<T> binding)
        {
            if (binding != null && s_bindings.Remove(binding))
                s_dirty = true;
        }

        public static void Raise(T eventData)
        {
            var snapshot = Snapshot();
            foreach (var binding in snapshot)
            {
                // Skip bindings deregistered by an earlier handler during this raise
                if (s_bindings.Contains(binding))
                    binding.OnEvent.Invoke(eventData);
            }
        }

        public static void Raise()
        {
            var snapshot = Snapshot();
            foreach (var binding in snapshot)
            {
                if (s_bindings.Contains(binding))
                    binding.OnEventNoArgs.Invoke();
            }
        }

        public static IReadOnlyList<IEventBinding<T>> GetBindings() => Snapshot();

        static IEventBinding<T>[] Snapshot()
        {
            if (s_dirty)
            {
                s_snapshot = new IEventBinding<T>[s_bindings.Count];
                s_bindings.CopyTo(s_snapshot);
                s_dirty = false;
            }
            return s_snapshot;
        }

        // Called via reflection by EventBusUtil.ClearAllBuses and the test suite.
        static void ClearBindings()
        {
            s_bindings.Clear();
            s_dirty = true;
        }
    }
}
