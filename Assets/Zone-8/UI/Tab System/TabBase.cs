using UnityEngine;
using UnityEngine.EventSystems;


namespace Zone8.UI.TabSystem
{
    public abstract class TabBase : MonoBehaviour, ITab, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {

        protected HighlightTabsManager _tabsManager;

        private void Awake()
        {
            _tabsManager = GetComponentInParent<HighlightTabsManager>();
            if (_tabsManager != null) _tabsManager.AddTab(this);

        }

        public virtual void Highlight() { }
        public virtual void Dehighlight() { }

        public abstract void ActivateContent();

        public abstract void DeactivateContent();


        public void OnPointerClick(PointerEventData eventData)
        {
            _tabsManager.SwitchTab(this);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tabsManager.GetActiveTab() != (ITab)this) Highlight();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tabsManager.GetActiveTab() != (ITab)this) Dehighlight();
        }
    }
}