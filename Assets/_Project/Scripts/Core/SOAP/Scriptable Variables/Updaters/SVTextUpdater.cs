using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

#if USING_lOCALIZATION
using UnityEngine.Localization;
#endif

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(TMP_Text))]
    public abstract class SVTextUpdater<T> : SVUpdaterBase<T, TMP_Text>
    {
        [Title("Text Settings")]
        [SerializeField]
        private bool _useLocalization;

#if USING_lOCALIZATION
        [ShowIf(nameof(_useLocalization))]
        [Title("Localization")]
        [SerializeField] private LocalizedString _localizedPrefix;

        [ShowIf(nameof(_useLocalization))]
        [SerializeField] private LocalizedString _localizedSuffix;
#endif

        [HideIf(nameof(_useLocalization))]
        [Title("Normal Text")]
        [SerializeField] private string _prefix;

        [HideIf(nameof(_useLocalization))]
        [SerializeField] private string _suffix;

        protected override void HideTarget()
        {
            _targetComponent.enabled = false;
        }

        protected override void UpdateTargetValue(T newValue)
        {
            string prefix = GetPrefix();
            string suffix = GetSuffix();

            _targetComponent.text = $"{prefix} {newValue?.ToString() ?? string.Empty} {suffix}";
        }


        private string GetPrefix()
        {
#if USING_lOCALIZATION
            if (_useLocalization && _localizedPrefix != null && !_localizedPrefix.IsEmpty)
            {
                return _localizedPrefix.GetLocalizedString();
            }
#endif
            return _prefix;
        }

        private string GetSuffix()
        {
#if USING_lOCALIZATION
            if (_useLocalization && _localizedSuffix != null && !_localizedSuffix.IsEmpty)
                return _localizedSuffix.GetLocalizedString();
#endif
            return _suffix;
        }
    }
}
