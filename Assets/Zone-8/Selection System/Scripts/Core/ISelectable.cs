using UnityEngine.Events;

namespace Zone8.Selection
{
    public interface ISelectable
    {
        public UnityAction<ISelectable> ItemSelected { get; set; }
        public UnityAction<ISelectable> ItemDeselected { get; set; }
        public bool IsSelected { get; }

        public void Select();
        public void Deselect();
    }
}