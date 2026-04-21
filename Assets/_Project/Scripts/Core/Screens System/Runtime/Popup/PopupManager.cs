using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using Zone8.SOAP.Events;

namespace Zone8.Screens
{
    public class PopupManager : MonoBehaviour, IEventListener<PopupEventArgs>
    {
        [SerializeField] PopupEventSO _popupEvent;

        [SerializeField] private GameObject _bgImage;
        [SerializeField] private Transform _popupRoot;

        private readonly Dictionary<PopupSO, Popup> _popupsDic = new();
        private readonly Stack<PopupSO> _popupsStack = new();

        private void Awake()
        {
            if (_popupRoot == null) _popupRoot = transform;
        }
        private void OnEnable()
        {
            _popupEvent.RegisterListener(this);
        }

        private void OnDisable()
        {
            _popupEvent.UnregisterListener(this);
        }

        private void Start()
        {
            if (_bgImage != null)
                _bgImage.gameObject.SetActive(false);
        }

        #region Public API

        [Button]
        public async Awaitable ShowPopup(PopupSO popupSO)
        {
            if (popupSO == null || popupSO.PopupPrefab == null)
            {
                Logger.LogError("PopupSO or Prefab is null.");
                return;
            }

            var instance = GetOrCreateInstance(popupSO);

            if (_popupsStack.Contains(popupSO))
                return;

            if (_popupsStack.Count == 0)
                ToogleBG(true);

            await instance.Show();
            _popupsStack.Push(popupSO);
        }

        [Button]
        public async Awaitable CloseTopPopup()
        {
            if (_popupsStack.Count == 0)
                return;

            var popupSO = _popupsStack.Pop();
            var instance = _popupsDic[popupSO];
            await instance.Hide();

            if (_popupsStack.Count == 0)
                ToogleBG(false);
        }

        [Button]
        public async Awaitable ClosePopup(PopupSO popupSO)
        {
            if (!_popupsDic.TryGetValue(popupSO, out var instance))
                return;

            if (_popupsStack.Contains(popupSO))
            {
                await instance.Hide();
                RemoveFromStack(popupSO);

                if (_popupsStack.Count == 0)
                    ToogleBG(false);
            }
        }

        #endregion

        #region Internal
        private void ToogleBG(bool show)
        {
            if (_bgImage == null) return;
            if (show)
            {
                _bgImage.transform.SetAsLastSibling();
                _bgImage.SetActive(true);
            }
            else
            {
                _bgImage.SetActive(false);
            }
        }

        private UIScreenBase GetOrCreateInstance(PopupSO popupSO)
        {
            if (_popupsDic.TryGetValue(popupSO, out var instance))
                return instance;

            instance = Instantiate(popupSO.PopupPrefab, _popupRoot);
            instance.gameObject.SetActive(false);
            _popupsDic.Add(popupSO, instance);

            return instance;
        }

        private void RemoveFromStack(PopupSO popupSO)
        {
            var temp = new Stack<PopupSO>();

            while (_popupsStack.Count > 0)
            {
                var item = _popupsStack.Pop();
                if (item != popupSO)
                    temp.Push(item);
            }

            while (temp.Count > 0)
                _popupsStack.Push(temp.Pop());
        }

        public void OnEventRaised(PopupEventArgs args)
        {
            if (args.Show)
            {
                ShowPopup(args.Popup);
            }
            else
            {
                ClosePopup(args.Popup);
            }
        }

        #endregion
    }
}
