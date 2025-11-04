using TMPro;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(TMP_Text))]
    public abstract class SVTextUpdater<T> : SVUpdaterBase<T, TMP_Text>
    {
        [SerializeField] private string _prefix = string.Empty;
        [SerializeField] private string _suffix = string.Empty;


        protected override void HideTarget()
        {
            _target.enabled = false;
        }

        protected override void UpdateTargetValue(T newValue)
        {
            if (_target != null)
                _target.text = $"{_prefix}{newValue?.ToString() ?? string.Empty}{_suffix}";
        }
    }
}
