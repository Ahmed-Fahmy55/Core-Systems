using TMPro;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(TMP_Text))]
    public class SVTextStringUpdater : SVTextUpdater<string>
    {
        public override void ResetTargetValue()
        {
            _targetComponent.text = _initialValue;
        }

        protected override string SetIntialValue()
        {
            return _targetComponent.text;
        }
    }
}
