using TMPro;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(TMP_Text))]
    public class SVTextStringUpdater : SVTextUpdater<string>
    {
        protected override bool IsVariableHasValue()
        {
            return _variable != null && !string.IsNullOrEmpty(_variable.Value);
        }
    }
}
