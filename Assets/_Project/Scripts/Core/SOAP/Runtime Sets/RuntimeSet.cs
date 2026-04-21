using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SOAP.RuntimeSet
{
    public abstract class RuntimeSet<T> : ScriptableObject
    {
        public event Action<T> OnItemAdded;
        public event Action<T> OnItemRemoved;

        [ShowInInspector]
        public List<T> Items = new();

        public void Add(T item)
        {
            if (!Items.Contains(item)) Items.Add(item);
            OnItemAdded?.Invoke(item);
        }

        public void Remove(T item)
        {
            if (Items.Contains(item)) Items.Remove(item);
            OnItemRemoved?.Invoke(item);
        }
    }
}
