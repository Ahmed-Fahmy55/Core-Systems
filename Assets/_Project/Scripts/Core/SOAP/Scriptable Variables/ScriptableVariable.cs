using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable
{
    public interface INullable
    {
        bool IsNull { get; }
    }

    [InlineEditor]
    public class ScriptableVariable<T> : ScriptableObject, INullable
    {
        public event Action<T> OnValueChanged;

        [SerializeField]
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (EqualityComparer<T>.Default.Equals(_value, value)) return;

                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }

        /// <summary>Raises <see cref="OnValueChanged"/> with the current value even though nothing changed.</summary>
        public void ForceNotify() => OnValueChanged?.Invoke(_value);

        public bool IsNull
        {
            get
            {
                if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                    return false;

                return _value == null;
            }
        }

#if UNITY_EDITOR
        // Play-mode writes to a ScriptableObject asset would otherwise persist in the
        // editor: snapshot the value when play begins and restore it when play ends.
        [NonSerialized] private T _valueBeforePlay;
        [NonSerialized] private bool _captured;

        private void OnEnable()
        {
            UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // Assets loaded during play (Addressables/Resources) miss EnteredPlayMode
            if (Application.isPlaying && !_captured)
            {
                _valueBeforePlay = _value;
                _captured = true;
            }
        }

        private void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.EnteredPlayMode && !_captured)
            {
                _valueBeforePlay = _value;
                _captured = true;
            }
            else if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode && _captured)
            {
                _value = _valueBeforePlay;
                _captured = false;
            }
        }
#endif
    }


    [Serializable]
    public struct ScriptableVariableRef<T> : INullable
    {
        public bool UseConstant;

        [ShowIf("@UseConstant == false")]
        [HideLabel]
        [SerializeField]
        private ScriptableVariable<T> Sv;

        [ShowIf("@UseConstant == true")]
        [HideLabel]
        [SerializeField]
        private T ConstValue;

        public T Value
        {
            get
            {
                if (UseConstant)
                    return ConstValue;

                if (Sv != null)
                    return Sv.Value;

                return default;
            }
            set
            {
                if (UseConstant)
                    ConstValue = value;
                else if (Sv != null)
                    Sv.Value = value;
            }
        }

        public bool IsNull
        {
            get
            {
                if (UseConstant)
                {
                    if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
                        return false;

                    return ConstValue == null;
                }

                return Sv == null || Sv.IsNull;
            }
        }
    }
}
