using Sirenix.OdinInspector;
using System;
using System.Data.SqlTypes;
using UnityEngine;



namespace Zone8.SOAP.ScriptableVariable
{

    [InlineEditor]
    public class ScriptableVariable<T> : ScriptableObject
    {
        public event Action<T> OnValueChanged;

        [SerializeField]
        private T _value;


        public T Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
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
                else if (Sv != null)
                    return Sv.Value;
                return default(T);
            }
            set
            {
                if (UseConstant)
                    ConstValue = value;
                else if (Sv != null)
                    Sv.Value = value;
            }
        }

        public bool IsNull { get => (Sv == null) && ConstValue == null; }
    }

}
