using UnityEngine;
using UnityEngine.UI;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(Image))]
    public class SVColorUpdater : SVUpdaterBase<Color, Image>
    {
        protected override void HideTarget()
        {
            _targetComponent.enabled = false;
        }

        public override void ResetTargetValue()
        {
            _targetComponent.color = _initialValue;
        }

        protected override Color SetIntialValue()
        {
            return _targetComponent.color;
        }

        protected override void UpdateTargetValue(Color newValue)
        {
            if (newValue != Color.clear)
                _targetComponent.color = newValue;
        }
    }
}
