using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using Zone8.Events;
using Zone8.UnityServices.Auth;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    public class SessionUIMediator : MonoBehaviour
    {
        [SerializeField] CanvasGroup _hostCanvasGroup;
        [SerializeField] CanvasGroup _joinCanvasGroup;
        [SerializeField] GameObject _loadingSpinner;

        MultiplayerServicesFacade _multiplayerServicesFacade;
        LocalSession _localSession;
        //ConnectionManager _connectionManager;

        EventBinding<ConnectionMessageEvent> _connectStatusBinding;

        const string k_DefaultSessionName = "no-name";

        private void Awake()
        {
            // _connectionManager = FindAnyObjectByType<ConnectionManager>();

            _connectStatusBinding = new(OnConnectStatus);
            EventBus<ConnectionMessageEvent>.Register(_connectStatusBinding);

        }

        private void Start()
        {
            _localSession = LocalSession.Instance;
            _multiplayerServicesFacade = MultiplayerServicesFacade.Instance;
        }

        void OnDestroy()
        {
            EventBus<ConnectionMessageEvent>.Deregister(_connectStatusBinding);
        }

        // Multiplayer Services SDK calls done from UI
        public async void CreateSessionRequest(string sessionName, bool isPrivate)
        {
            // before sending request, populate an empty session name, if necessary
            if (string.IsNullOrEmpty(sessionName))
            {
                sessionName = k_DefaultSessionName;
            }

            BlockUIWhileLoadingIsInProgress();

            var playerIsAuthorized = await AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            // _connectionManager.StartHostSession();

            var result = await _multiplayerServicesFacade.TryCreateSessionAsync(sessionName, isPrivate);

            HandleSessionJoinResult(result);
        }

        public async void QuerySessionRequest(bool blockUI)
        {
            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                return;
            }

            if (blockUI)
            {
                BlockUIWhileLoadingIsInProgress();
            }

            var playerIsAuthorized = await AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (blockUI && !playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            await _multiplayerServicesFacade.RetrieveAndPublishSessionListAsync();

            if (blockUI)
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }

        public async void JoinSessionWithCodeRequest(string sessionCode)
        {
            BlockUIWhileLoadingIsInProgress();

            var playerIsAuthorized = await AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            // _connectionManager.StartClientSession();

            var result = await _multiplayerServicesFacade.TryJoinSessionByCodeAsync(sessionCode);

            HandleSessionJoinResult(result);
        }

        public async void JoinSessionRequest(ISessionInfo sessionInfo)
        {
            BlockUIWhileLoadingIsInProgress();

            var playerIsAuthorized = await AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            //_connectionManager.StartClientSession();


            var result = await _multiplayerServicesFacade.TryJoinSessionByIdAsync(sessionInfo.Id);

            HandleSessionJoinResult(result);
        }

        public async void QuickJoinRequest()
        {
            BlockUIWhileLoadingIsInProgress();

            var playerIsAuthorized = await AuthenticationServiceFacade.EnsurePlayerIsAuthorized();

            if (!playerIsAuthorized)
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }

            //_connectionManager.StartHostSession();

            var result = await _multiplayerServicesFacade.TryQuickJoinSessionAsync();

            HandleSessionJoinResult(result);
        }

        void HandleSessionJoinResult((bool Success, ISession Session) result)
        {
            if (result.Success)
            {
                OnJoinedSession(result.Session);
            }
            else
            {
                // _connectionManager.RequestShutdown();
                UnblockUIAfterLoadingIsComplete();
            }
        }

        void OnJoinedSession(ISession remoteSession)
        {
            _multiplayerServicesFacade.SetRemoteSession(remoteSession);

            Logger.Log($"Joined session with ID: {_localSession.SessionID}");

            // _connectionManager.StartClientSession();
        }


        void BlockUIWhileLoadingIsInProgress()
        {
            _hostCanvasGroup.interactable = false;
            _joinCanvasGroup.interactable = false;
            _loadingSpinner.SetActive(true);
        }

        void UnblockUIAfterLoadingIsComplete()
        {
            // this callback can happen after we've already switched to a different scene
            // in that case the canvas group would be null
            if (_hostCanvasGroup != null)
            {
                _hostCanvasGroup.interactable = true;
                _joinCanvasGroup.interactable = true;
                _loadingSpinner.SetActive(false);
            }
        }

        void OnConnectStatus(ConnectionMessageEvent status)
        {
            if (status.ConnectStatus is ConnectStatus.GenericDisconnect or ConnectStatus.StartClientFailed)
            {
                UnblockUIAfterLoadingIsComplete();
            }
        }
    }
}
