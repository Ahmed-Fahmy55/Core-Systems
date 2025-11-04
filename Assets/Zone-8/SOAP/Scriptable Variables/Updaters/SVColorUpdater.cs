using UnityEngine;
using UnityEngine.UI;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(Image))]
    public class SVColorUpdater : SVUpdaterBase<Color, Image>
    {

        protected override bool IsVariableHasValue()
        {
            return _variable != null && _variable.Value != null && _variable.Value != Color.clear;
        }

        protected override void HideTarget()
        {
            _target.enabled = false;
        }

        protected override void UpdateTargetValue(Color newValue)
        {
            if (_target != null)
                _target.color = newValue;
        }
    }
}
