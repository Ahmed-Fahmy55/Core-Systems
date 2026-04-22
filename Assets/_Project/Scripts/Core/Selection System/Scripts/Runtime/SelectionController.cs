using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Zone8.Selection
{
    public class SelectionController : SerializedMonoBehaviour, ISelectionHandler
    {
        public event Action NoItemSelected;
        public event Action<ISelectable> ItemSelected;
        public event Action<ISelectable> ItemDeselected;
        public event Action<List<ISelectable>> SelectionCompleted;

        [SerializeField] private bool _autoSubmitOnFirstSelection = false;
        [SerializeField] private bool _allowMultiSelect = false;
        [SerializeField] private bool _resetSelectionOnComplete = true;

        [SerializeField] private ISelectable _defaultSelectable;
        [SerializeField] private HashSet<ISelectable> _selectables = new();

        private readonly List<ISelectable> _selectedItems = new();

        private void Awake() => SubscribeToSelectables();

        private void Start()
        {
            if (_defaultSelectable != null) _defaultSelectable.Select();
            else NoItemSelected?.Invoke();
        }

        private void OnDestroy() => UnsubscribeFromSelectables();

        #region API

        public void AddSelectable(ISelectable selectable)
        {
            if (selectable != null && _selectables.Add(selectable))
                SubscribeToSelectable(selectable);
        }

        public void RemoveSelectable(ISelectable selectable)
        {
            if (selectable != null && _selectables.Remove(selectable))
            {
                UnsubscribeFromSelectable(selectable);
                if (_selectedItems.Contains(selectable)) selectable.Deselect();
            }
        }

        public void ResetSelection()
        {
            // Use a temporary list to avoid "Collection Modified" errors during iteration
            var toDeselect = _selectedItems.ToList();
            foreach (var item in toDeselect)
            {
                item.Deselect();
            }
            _selectedItems.Clear();
        }

        public void CompleteSelection()
        {
            SelectionCompleted?.Invoke(_selectedItems);
            if (_resetSelectionOnComplete) ResetSelection();
        }

        #endregion

        private void SubscribeToSelectable(ISelectable selectable)
        {
            if (selectable == null) return;
            selectable.ItemSelected += OnItemRequestedSelect;
            selectable.ItemDeselected += OnItemRequestedDeselect;
        }

        private void UnsubscribeFromSelectable(ISelectable selectable)
        {
            if (selectable == null) return;
            selectable.ItemSelected -= OnItemRequestedSelect;
            selectable.ItemDeselected -= OnItemRequestedDeselect;
        }

        private void OnItemRequestedSelect(ISelectable selectable)
        {
            if (_selectedItems.Contains(selectable)) return;

            if (!_allowMultiSelect && _selectedItems.Count > 0)
            {
                ResetSelection();
            }

            _selectedItems.Add(selectable);
            ItemSelected?.Invoke(selectable);

            if (_autoSubmitOnFirstSelection) CompleteSelection();
        }

        private void OnItemRequestedDeselect(ISelectable selectable)
        {
            if (_selectedItems.Remove(selectable))
            {
                ItemDeselected?.Invoke(selectable);
                if (_selectedItems.Count == 0) NoItemSelected?.Invoke();
            }
        }

        private void SubscribeToSelectables() { foreach (var s in _selectables) SubscribeToSelectable(s); }
        private void UnsubscribeFromSelectables() { foreach (var s in _selectables) UnsubscribeFromSelectable(s); }
    }
}