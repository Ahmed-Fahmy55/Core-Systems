using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zone8.SOAP.AssetVariable;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{
    public abstract class SVUpdaterBase<T, C> : MonoBehaviour where C : Component
    {
        [SerializeField] protected AssetVariableRef<ScriptableVariable<T>> _variable;
        [Space]
        [SerializeField] protected bool _updateOnStart = true;
        [SerializeField] protected bool _updateOnValueChange = true;
        [SerializeField] protected bool _hideOnNoValue;
        [SerializeField] protected bool _resetOnDisable;

        protected C _targetComponent;
        protected T _initialValue;

        protected virtual void Awake()
        {
            GetTarget();
            if (_targetComponent == null)
            {
                Logger.LogError($"No target component of type {typeof(C)} found on {gameObject.name}. Please assign a target component.", this);
                return;
            }
            _initialValue = SetIntialValue();

        }

        private void Start()
        {
            Init();
        }

        private void OnDisable()
        {
            if (_resetOnDisable)
            {
                ResetTargetValue();
            }
        }

        private void OnDestroy()
        {
            if (_variable.IsNull) return;

            _variable.Asset.OnValueChanged -= UpdateValue;
            _variable.ReleaseAsset();
        }

        private void Init()
        {
            if (_variable.Source == AssetSource.Addressable)
            {
                var handle = _variable.LoadAssetAsync();
                handle.Completed += OnAssetLoaded;
            }
            else
            {
                if (!_variable.Asset.IsNull)
                {
                    if (_updateOnStart) UpdateValue(_variable.Asset.Value);
                    if (_updateOnValueChange) _variable.Asset.OnValueChanged += UpdateValue;
                }
            }
        }

        private void OnAssetLoaded(AsyncOperationHandle<ScriptableVariable<T>> handle)
        {
            if (_updateOnStart) UpdateValue(_variable.Asset.Value);
            if (_updateOnValueChange) _variable.Asset.OnValueChanged += UpdateValue;
        }

        protected void UpdateValue(T newValue)
        {
            if (_hideOnNoValue && newValue == null)
            {
                HideTarget();
                return;
            }

            if (_targetComponent != null) UpdateTargetValue(newValue);
        }

        protected virtual void GetTarget()
        {
            if (_targetComponent == null)
            {
                _targetComponent = GetComponent<C>();
            }
        }

        protected abstract void HideTarget();

        protected abstract T SetIntialValue();

        protected abstract void UpdateTargetValue(T newValue);

        [Button]
        public abstract void ResetTargetValue();
    }
}
