using TMPro;
using UnityEngine;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{
    [RequireComponent(typeof(TMP_Text))]
    public class SVIntTextUpdater : SVTextUpdater<int>
    {
        public override void ResetTargetValue()
        {
            _targetComponent.text = _initialValue.ToString();
        }

        protected override int SetIntialValue()
        {
            if (string.IsNullOrEmpty(_targetComponent.text))
                return 0;

            int value = 0;
            int.TryParse(_targetComponent.text, out value);
            return value;
        }
    }
}