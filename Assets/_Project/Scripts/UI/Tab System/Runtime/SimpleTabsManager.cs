using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zone8.Audio;

namespace Zone8.UI.TabSystem
{
    public class SimpleTabsManager : SerializedMonoBehaviour, ITabManager
    {
        #region Events
        public event Action<ITab> TabSwitchedEvent;
        #endregion

        #region Serialized Fields
        [Title("Setup")]
        [SerializeField] private List<ITab> _tabs = new();

        [Title("Audio Feedback")]
        [SerializeField] private SFXClipSo _switchSound;
        #endregion

        #region Private Variables
        [ShowInInspector, ReadOnly, BoxGroup("State")]
        private ITab _activeTab;

        #endregion

        void Start()
        {
            if (_tabs.Count == 0)
            {
                Debug.LogWarning($"[SimpleTabsManager] No tabs assigned on {gameObject.name}");
                return;
            }

            foreach (var tab in _tabs)
            {
                tab.Dehighlight();
                tab.DeactivateContent();
            }

            SwitchTab(_tabs[0], true);
        }

        #region Public Methods

        [Button("Switch To Next Tab")]
        public void SwitchToNextTab()
        {
            if (_tabs.Count <= 1) return;

            int currentIndex = _tabs.IndexOf(_activeTab);
            int nextIndex = (currentIndex + 1) % _tabs.Count;
            SwitchTab(_tabs[nextIndex]);
        }

        [Button("Switch To Index")]
        public void SwitchToIndex(int index)
        {
            if (index < 0 || index >= _tabs.Count)
            {
                Debug.LogWarning("[SimpleTabsManager] Invalid tab index!");
                return;
            }
            SwitchTab(_tabs[index]);
        }

        /// <summary>
        /// Deep-linking: Switch to a tab using its unique String ID.
        /// </summary>
        [Button("Switch To ID")]
        public void SwitchTabByID(string id)
        {
            ITab target = _tabs.FirstOrDefault(t => t.TabID == id);
            if (target != null)
            {
                SwitchTab(target);
            }
            else
            {
                Debug.LogWarning($"[SimpleTabsManager] Tab with ID '{id}' not found.");
            }
        }

        public void SwitchTab(ITab tab) => SwitchTab(tab, false);

        private void SwitchTab(ITab tab, bool silent)
        {
            if (tab == null || _activeTab == tab) return;

            _activeTab?.DeactivateContent();
            _activeTab?.Dehighlight();

            _activeTab = tab;
            _activeTab.ActivateContent();
            _activeTab.Highlight();

            // Feedback
            if (!silent && _switchSound != null)
            {
                _switchSound.Play();
            }

            TabSwitchedEvent?.Invoke(_activeTab);
        }

        public void AddTab(ITab tab)
        {
            if (tab != null && !_tabs.Contains(tab))
            {
                _tabs.Add(tab);
            }
        }

        public void RemoveTab(ITab tab)
        {
            if (_tabs.Contains(tab))
            {
                _tabs.Remove(tab);
            }
        }

        public ITab GetActiveTab() => _activeTab;
        #endregion
    }
}