using UnityEngine;
using UnityEngine.UI;

namespace Zone8.SOAP.ScriptableVariable.Updaters
{

    [RequireComponent(typeof(Image))]
    public class SVImageUpdater : SVUpdaterBase<Sprite, Image>
    {

        protected override void HideTarget()
        {
            if (_targetComponent == null)
            {
                Logger.LogError($"No target component of type name : {gameObject.name}", this);
                return;
            }
            _targetComponent.enabled = false;
        }

        public override void ResetTargetValue()
        {
            if (_targetComponent == null)
            {
                Logger.LogError($"No target component of type name : {gameObject.name}", this);
                return;
            }
            _targetComponent.sprite = _initialValue;
        }

        protected override Sprite SetIntialValue()
        {
            return _targetComponent.sprite;
        }

        protected override void UpdateTargetValue(Sprite newValue)
        {
            if (newValue != null)
                _targetComponent.sprite = newValue;
        }
    }
}
