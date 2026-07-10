using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Zone8.Selection
{
    public abstract class UISelectableBase : MonoBehaviour, ISelectable, IPointerClickHandler
    {
        public UnityAction<ISelectable> ItemSelected { get; set; }
        public UnityAction<ISelectable> ItemDeselected { get; set; }

        public bool IsSelected { get; protected set; }

        public void Select()
        {
            if (IsSelected) return;

            IsSelected = true;
            OnSelect();
            ItemSelected?.Invoke(this);
        }

        public void Deselect()
        {
            if (!IsSelected) return;

            IsSelected = false;
            OnDeselect();
            ItemDeselected?.Invoke(this);
        }

        protected virtual void OnSelect()
        {
            //AnyExtraLogic
        }

        protected virtual void OnDeselect()
        {
            //AnyExtraLogic
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsSelected)
            {
                Deselect();
            }
            else
            {
                Select();
            }
        }
    }

}