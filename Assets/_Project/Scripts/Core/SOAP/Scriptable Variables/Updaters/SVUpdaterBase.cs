using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{
    public abstract class SVUpdaterBase<T, C> : MonoBehaviour where C : Component
    {
        [SerializeField] protected ScriptableVariable<T> _variable;
        [Space]
        [SerializeField] protected bool _updateOnEnable = true;
        [SerializeField] protected bool _updateOnValueChange = true;
        [SerializeField] protected bool _hideOnNoValue;

        protected C _target;

        protected virtual void Awake()
        {
            GetTarget();
            if (_target == null)
            {
                Debug.LogError($"No target component of type {typeof(C)} found on {gameObject.name}. Please assign a target component.", this);
            }
        }

        private void OnEnable()
        {
            if (_variable != null && _updateOnValueChange) _variable.OnValueChanged += UpdateValue;

            if (_updateOnEnable && _variable != null) UpdateValue(_variable.Value);
        }

        private void OnDisable()
        {
            if (_variable != null && _updateOnValueChange)
                _variable.OnValueChanged -= UpdateValue;
        }

        public void UpdateValue(T newValue)
        {
            if (_hideOnNoValue && !IsVariableHasValue()) HideTarget();
            UpdateTargetValue(newValue);
        }

        protected abstract bool IsVariableHasValue();

        protected abstract void HideTarget();

        protected abstract void UpdateTargetValue(T newValue);

        protected virtual void GetTarget()
        {
            if (_target == null)
            {
                _target = GetComponent<C>();
            }
        }
    }
}
