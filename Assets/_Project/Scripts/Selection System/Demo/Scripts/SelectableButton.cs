using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Selection
{
    public class SelectableButton : SelectableBase
    {
        Button _button;

        Color _color;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _color = _button.image.color;
        }

        protected override void OnSelect()
        {
            _button.image.color = Color.blue;
        }

        protected override void OnDeselect()
        {
            _button.image.color = _color;
        }
    }
}
