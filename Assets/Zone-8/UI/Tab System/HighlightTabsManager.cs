using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zone8.UI.TabSystem
{
    public class HighlightTabsManager : SerializedMonoBehaviour, ITabManager
    {

        #region Serialized Fields
        [SerializeField] private List<TabBase> _tabs = new();
        #endregion

        #region private variable
        private TabBase _activeTab;
        #endregion

        #region public Events
        public event Action<TabBase> TabSwitchedEvent;
        #endregion

        void Start()
        {
            foreach (var tab in _tabs)
            {
                tab.Dehighlight();
                tab.DeactivateContent();
            }

            _activeTab = _tabs[0];
            _activeTab.ActivateContent();
            _activeTab.Highlight();
        }

        #region public methods
        [Button("Switch To Next Tab")]
        public void SwitchToNextTab()
        {
            if (_tabs.Count == 0) return;

            int currentIndex = _tabs.IndexOf((TabBase)_activeTab);
            int nextIndex = (currentIndex + 1) % _tabs.Count;
            SwitchToTab(nextIndex);
        }

        public void AddTab(ITab tab)
        {
            if (!_tabs.Contains((TabBase)tab))
            {
                _tabs.Add((TabBase)tab);
            }
        }

        public void RemoveTab(ITab tab)
        {
            if (_tabs.Contains((TabBase)tab))
            {
                _tabs.Remove((TabBase)tab);
            }
        }

        [Button("Switch To Tab")]
        public void SwitchToTab(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                Logger.LogWarning("Invalid tab index!");
                return;
            }
            if (_activeTab == _tabs[index]) return;

            _activeTab.DeactivateContent();
            _activeTab.Dehighlight();
            _activeTab = _tabs[index];
            _activeTab.ActivateContent();
            _activeTab.Highlight();
            Logger.Log($"Switched to tab: {_activeTab}");

            TabSwitchedEvent?.Invoke(_activeTab);
        }

        public ITab GetActiveTab() => _activeTab;

        public void SwitchTab(ITab tab)
        {
            if (tab == null)
            {
                Logger.LogWarning("Invalid tab index!");
                return;
            }
            if (_activeTab == (TabBase)tab) return;

            _activeTab.DeactivateContent();
            _activeTab.Dehighlight();
            _activeTab = (TabBase)tab;
            _activeTab.ActivateContent();
            _activeTab.Highlight();
            Logger.Log($"Switched to tab: {_activeTab}");

            TabSwitchedEvent?.Invoke(_activeTab);
        }
        #endregion
    }
}