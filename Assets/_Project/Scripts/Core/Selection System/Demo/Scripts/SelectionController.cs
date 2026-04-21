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


        [SerializeField] private bool _autoSubmitOnFirstSelection = true;
        [SerializeField] private bool _allowMultiSelect = true;
        [SerializeField] private bool _resetSelectionOnComplete = true;

        [SerializeField] private ISelectable _defaultSelectable;
        [SerializeField] private HashSet<ISelectable> _selectables;

        private List<ISelectable> _selectedItems = new();

        private void Awake()
        {
            if (_selectables == null)
            {
                _selectables = new HashSet<ISelectable>();
            }

            SubscribeToSelectables();
        }

        private void OnEnable()
        {
            if (_defaultSelectable == null)
                NoItemSelected?.Invoke();
        }

        private void Start()
        {
            if (_defaultSelectable != null)
            {
                _defaultSelectable.Select();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromSelectables();
        }

        #region API

        public void AddSelectable(ISelectable selectable)
        {
            if (_selectables.Add(selectable))
            {
                SubscribeToSelectable(selectable);
            }
        }

        public void RemoveSelectable(ISelectable selectable)
        {
            if (_selectables.Remove(selectable))
            {
                UnsubscribeFromSelectable(selectable);
            }
        }

        public void ResetSelection()
        {
            foreach (var item in _selectedItems.ToList())
            {
                item.Deselect();
            }
            _selectedItems.Clear();
        }

        public void CompleteSelection()
        {
            SelectionCompleted?.Invoke(_selectedItems);

            if (_resetSelectionOnComplete)
            {
                ResetSelection();
            }
        }

        #endregion

        #region Private Methods

        private void SubscribeToSelectables()
        {
            if (_selectables == null) return;

            foreach (var selectable in _selectables)
            {
                SubscribeToSelectable(selectable);
            }
        }

        private void UnsubscribeFromSelectables()
        {
            if (_selectables == null) return;

            foreach (var selectable in _selectables)
            {
                UnsubscribeFromSelectable(selectable);
            }
        }

        private void SubscribeToSelectable(ISelectable selectable)
        {
            if (selectable == null)
            {
                Logger.LogError("Selectable is null");
                return;
            }
            selectable.ItemSelected += Select;
            selectable.ItemDeselected += Deselect;
        }

        private void UnsubscribeFromSelectable(ISelectable selectable)
        {
            if (selectable == null)
            {
                Logger.LogError("Selectable is null");
                return;
            }

            selectable.ItemSelected -= Select;
            selectable.ItemDeselected -= Deselect;
        }

        private void Select(ISelectable selectable)
        {
            if (!_allowMultiSelect)
            {
                ResetSelection();
            }

            if (!_selectedItems.Contains(selectable))
            {
                _selectedItems.Add(selectable);
                ItemSelected?.Invoke(selectable);

                if (_autoSubmitOnFirstSelection)
                {
                    CompleteSelection();
                }
            }
        }

        private void Deselect(ISelectable selectable)
        {
            if (_selectedItems.Remove(selectable))
            {
                ItemDeselected?.Invoke(selectable);

                if (_selectedItems.Count == 0)
                {
                    NoItemSelected?.Invoke();
                }
            }
        }

        #endregion


        ///////////////////////////////////////////////////////////////

        [Button("Populate Selectables")]
        private void Populate()
        {
            _selectables.Clear();
            foreach (Transform item in transform)
            {
                if (item.TryGetComponent(out ISelectable selectable))
                {
                    _selectables.Add(selectable);
                    _defaultSelectable = selectable;
                }
            }
        }

    }
}
