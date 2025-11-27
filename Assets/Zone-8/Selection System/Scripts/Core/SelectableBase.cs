using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Zone8.Selection
{
    public abstract class SelectableBase : MonoBehaviour, ISelectable, IPointerClickHandler
    {
        public UnityAction<ISelectable> ItemSelected { get; set; }
        public UnityAction<ISelectable> ItemDeselected { get; set; }

        public bool IsSelected { get; protected set; }

        public void Select()
        {
            IsSelected = true;
            OnSelect();
            ItemSelected?.Invoke(this);
        }

        public void Deselect()
        {
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