using Michsky.UI.Heat;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using Zone8.Events;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    /// <summary>
    /// Handles the list of SessionListItemUIs and ensures it stays synchronized with the Session list from the service.
    /// </summary>
    public class SessionJoiningUI : MonoBehaviour
    {
        [SerializeField] SessionListItemUI _sessionListItemPrefab;
        [SerializeField] Transform _sessionListContainer;
        [SerializeField] Graphic _emptySessionListLabel;
        [SerializeField] ButtonManager _refreshButton;


        SessionUIMediator _sessionUIMediator;
        EventBinding<SessionListFetchedMessage> _localSessionsRefreshedBind;
        List<SessionListItemUI> _sessionListItems = new List<SessionListItemUI>();
        Coroutine _updateUiRoutine;


        void Awake()
        {
            _sessionUIMediator = FindAnyObjectByType<SessionUIMediator>();
            _refreshButton.onClick.AddListener(OnRefresh);
            _localSessionsRefreshedBind = new(UpdateUI);
            EventBus<SessionListFetchedMessage>.Register(_localSessionsRefreshedBind);
        }

        void OnDestroy()
        {
            _refreshButton.onClick.RemoveListener(OnRefresh);
            if (_localSessionsRefreshedBind != null)
            {
                EventBus<SessionListFetchedMessage>.Deregister(_localSessionsRefreshedBind);
            }
        }

        private void OnEnable()
        {
            if (_updateUiRoutine != null) StopCoroutine(_updateUiRoutine);
            _updateUiRoutine = StartCoroutine(PeriodicRefresh());
        }

        private void OnDisable()
        {

            if (_updateUiRoutine != null) StopCoroutine(_updateUiRoutine);
        }

        private void OnRefresh()
        {
            _sessionUIMediator.QuerySessionRequest(true);
        }

        IEnumerator PeriodicRefresh()
        {
            while (true)
            {
                _sessionUIMediator.QuerySessionRequest(true);
                yield return new WaitForSeconds(5f);
            }
        }

        void UpdateUI(SessionListFetchedMessage message)
        {
            Logger.Log($"SessionJoiningUI: UpdateUI called with {message.LocalSessions.Count} sessions.");
            EnsureNumberOfActiveUISlots(message.LocalSessions.Count);

            for (var i = 0; i < message.LocalSessions.Count; i++)
            {
                var localSession = message.LocalSessions[i];
                _sessionListItems[i].SetData(localSession, _sessionUIMediator);
            }

            _emptySessionListLabel.enabled = message.LocalSessions.Count == 0;
        }

        void EnsureNumberOfActiveUISlots(int requiredNumber)
        {
            int delta = requiredNumber - _sessionListItems.Count;

            for (int i = 0; i < delta; i++)
            {
                _sessionListItems.Add(CreateSessionListItem());
            }

            for (int i = 0; i < _sessionListItems.Count; i++)
            {
                _sessionListItems[i].gameObject.SetActive(i < requiredNumber);
            }
        }

        SessionListItemUI CreateSessionListItem()
        {
            SessionListItemUI listItem = Instantiate(_sessionListItemPrefab, _sessionListContainer);
            listItem.gameObject.SetActive(false);
            return listItem;
        }

        private string SanitizeJoinCode(string dirtyString)
        {
            return Regex.Replace(dirtyString.ToUpper(), "[^A-Z0-9]", "");
        }
    }
}
