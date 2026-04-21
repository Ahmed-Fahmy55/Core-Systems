using UnityEngine;
using UnityEngine.UI;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(Image))]
    public class SVImageUpdater : SVUpdaterBase<Sprite, Image>
    {

        protected override void HideTarget()
        {
            _target.enabled = false;
        }

        protected override bool IsVariableHasValue()
        {
            return _variable != null && _variable.Value != null;
        }

        protected override void UpdateTargetValue(Sprite newValue)
        {
            if (_target != null)
                _target.sprite = newValue;
        }
    }
}
