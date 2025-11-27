using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Zone8.Selection
{
    public class UISubmitButton : SerializedMonoBehaviour
    {
        [SerializeField] private ISelectionHandler _selectionController;
        [SerializeField] private bool _hideButtonOnNoSelection;

        private Button _submitButton;

        private void Awake()
        {
            InitializeButton();
            InitializeSelectionController();
        }

        private void Start()
        {
            SubscribeToSelectionEvents();
            UpdateButtonVisibility();
        }

        private void OnDestroy()
        {
            UnsubscribeFromSelectionEvents();
        }

        public void SetSelectionController(SelectionController selectionController)
        {
            if (_selectionController == selectionController) return;

            UnsubscribeFromSelectionEvents();
            _selectionController = selectionController;
            SubscribeToSelectionEvents();
        }

        public void SubmitSelection()
        {
            if (_selectionController != null)
            {
                _selectionController.CompleteSelection();
            }
            else
            {
                Logger.LogError("Cannot submit selection: SelectionController is null.", this);
            }
        }

        private void InitializeButton()
        {
            _submitButton = GetComponent<Button>();
            if (_submitButton != null)
            {
                _submitButton.onClick.AddListener(SubmitSelection);
            }
        }

        private void InitializeSelectionController()
        {
            if (_selectionController == null)
            {
                _selectionController = GetComponentInParent<ISelectionHandler>();
            }
        }

        private void SubscribeToSelectionEvents()
        {
            if (_selectionController == null) return;

            _selectionController.NoItemSelected += OnNoItemSelected;
            _selectionController.ItemSelected += OnItemSelected;
        }

        private void UnsubscribeFromSelectionEvents()
        {
            if (_selectionController == null) return;

            _selectionController.NoItemSelected -= OnNoItemSelected;
            _selectionController.ItemSelected -= OnItemSelected;
        }

        private void OnItemSelected(ISelectable selectable)
        {
            SetButtonVisibility(true);
        }

        private void OnNoItemSelected()
        {
            if (_hideButtonOnNoSelection)
            {
                SetButtonVisibility(false);
            }
        }

        private void UpdateButtonVisibility()
        {
            if (_submitButton != null)
            {
                OnNoItemSelected();
            }
        }

        private void SetButtonVisibility(bool isVisible)
        {
            if (_submitButton != null)
            {
                _submitButton.gameObject.SetActive(isVisible);
            }
        }
    }
}
