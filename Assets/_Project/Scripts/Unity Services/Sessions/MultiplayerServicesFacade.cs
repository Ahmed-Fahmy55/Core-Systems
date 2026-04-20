using System;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;
using Zone8.Events;
using Zone8.UnityServices.infastructure;
using Zone8.Utilities;

namespace Zone8.UnityServices.Sessions
{
    /// <summary>
    /// An abstraction layer between the direct calls into the Multiplayer Services SDK and the outcomes you actually want.
    /// </summary>
    public class MultiplayerServicesFacade : Singleton<MultiplayerServicesFacade>
    {
        [SerializeField] int _maxPlayersPerSession;
        [SerializeField] int _maxSessionsToShow;
        [SerializeField] EConnectionType _connectionType = EConnectionType.Relay;



        LocalSession _localSession;
        LocalSessionUser _localUser;

        MultiplayerServicesInterface _multiplayerServicesInterface;

        RateLimitCooldown _rateLimitQuery;
        RateLimitCooldown _rateLimitJoin;
        RateLimitCooldown _rateLimitQuickJoin;
        RateLimitCooldown _rateLimitHost;

        public ISession CurrentUnitySession { get; private set; }

        bool m_IsTracking;


        protected override void Awake()
        {
            base.Awake();
            _multiplayerServicesInterface = new MultiplayerServicesInterface(_maxPlayersPerSession, _maxSessionsToShow, connectionType: _connectionType);
        }

        private void Start()
        {
            _localUser = LocalSessionUser.Instance;
            _localSession = LocalSession.Instance;

            //See https://docs.unity.com/ugs/manual/lobby/manual/rate-limits
            _rateLimitQuery = new RateLimitCooldown(1f);
            _rateLimitJoin = new RateLimitCooldown(1f);
            _rateLimitQuickJoin = new RateLimitCooldown(1f);
            _rateLimitHost = new RateLimitCooldown(3f);
        }

        private void OnDestroy()
        {
            EndTracking();
        }

        public void SetRemoteSession(ISession session)
        {
            CurrentUnitySession = session;
            _localSession.ApplyRemoteData(session);
        }

        /// <summary>
        /// Initiates tracking of joined session's events. The host also starts sending heartbeat pings here.
        /// </summary>
        public void BeginTracking()
        {
            if (!m_IsTracking)
            {
                m_IsTracking = true;
                SubscribeToJoinedSession();
            }
        }

        /// <summary>
        /// Ends tracking of joined session's events and leaves or deletes the session. The host also stops sending heartbeat
        /// pings here.
        /// </summary>
        public void EndTracking()
        {
            if (m_IsTracking)
            {
                m_IsTracking = false;
            }

            if (CurrentUnitySession != null)
            {
                UnsubscribeFromJoinedSession();
                if (_localUser.IsHost)
                {
                    DeleteSessionAsync();
                }
                else
                {
                    LeaveSessionAsync();
                }
            }
        }

        /// <summary>
        /// Attempt to create a new session and then join it.
        /// </summary>
        public async Task<(bool Success, ISession Session)> TryCreateSessionAsync(string sessionName, bool isPrivate)
        {
            if (!_rateLimitHost.CanCall)
            {
                Logger.LogWarning("Create Session hit the rate limit.");
                return (false, null);
            }

            try
            {
                var sessionOptions = new SessionOptions
                {
                    Name = sessionName,
                    IsPrivate = isPrivate,
                    MaxPlayers = _maxPlayersPerSession,
                    PlayerProperties = _localUser.GetDataForUnityServices()
                };

                var session = await _multiplayerServicesInterface.CreateSession(sessionOptions);
                return (true, session);
            }
            catch (Exception e)
            {
                PublishError(e);
            }

            return (false, null);
        }

        public async Task<(bool Success, ISession Session)> TryCreateOrJoinSessionAsync(string sessionId, string sessionName, bool isPrivate)
        {
            if (!_rateLimitHost.CanCall)
            {
                Logger.LogWarning("Create Session hit the rate limit.");
                return (false, null);
            }

            try
            {
                var sessionOptions = new SessionOptions
                {
                    Name = sessionName,
                    IsPrivate = isPrivate,
                    MaxPlayers = _maxPlayersPerSession,
                    PlayerProperties = _localUser.GetDataForUnityServices()
                };

                var session = await _multiplayerServicesInterface.CreateOrJoinSession(sessionId, sessionOptions);
                return (true, session);
            }
            catch (Exception e)
            {
                PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Attempt to join an existing session with a join code.
        /// </summary>
        public async Task<(bool Success, ISession Session)> TryJoinSessionByCodeAsync(string sessionCode)
        {
            if (!_rateLimitJoin.CanCall)
            {
                Logger.LogWarning("Join Session hit the rate limit.");
                return (false, null);
            }

            if (string.IsNullOrEmpty(sessionCode))
            {
                Logger.LogWarning("Cannot join a Session without a join code.");
                return (false, null);
            }

            Logger.Log($"Joining session with join code {sessionCode}");

            try
            {
                var session = await _multiplayerServicesInterface.JoinSessionByCode(sessionCode, _localUser.GetDataForUnityServices());
                return (true, session);
            }
            catch (Exception e)
            {
                PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Attempt to join an existing session by name.
        /// </summary>
        public async Task<(bool Success, ISession Session)> TryJoinSessionByIdAsync(string sessionId)
        {
            if (!_rateLimitJoin.CanCall)
            {
                Debug.LogWarning("Join Session hit the rate limit.");
                return (false, null);
            }

            if (string.IsNullOrEmpty(sessionId))
            {
                Debug.LogWarning("Cannot join a Session without a session name.");
                return (false, null);
            }

            Debug.Log($"Joining session with name {sessionId}");

            try
            {
                var session = await _multiplayerServicesInterface.JoinSessionById(sessionId, _localUser.GetDataForUnityServices());
                return (true, session);
            }
            catch (Exception e)
            {
                PublishError(e);
            }

            return (false, null);
        }

        /// <summary>
        /// Attempt to join the first session among the available sessions that match the filtered onlineMode.
        /// </summary>
        public async Task<(bool Success, ISession Session)> TryQuickJoinSessionAsync()
        {
            if (!_rateLimitQuickJoin.CanCall)
            {
                Debug.LogWarning("Quick Join Session hit the rate limit.");
                return (false, null);
            }

            try
            {
                var session = await _multiplayerServicesInterface.QuickJoinSession(_localUser.GetDataForUnityServices());
                return (true, session);
            }
            catch (Exception e)
            {
                PublishError(e);
            }

            return (false, null);
        }

        void ResetSession()
        {
            CurrentUnitySession = null;
            _localUser?.ResetState();
            _localSession?.Reset(_localUser);

            // no need to disconnect Netcode, it should already be handled by Netcode's callback to disconnect
        }

        void SubscribeToJoinedSession()
        {
            CurrentUnitySession.Changed += OnSessionChanged;
            CurrentUnitySession.StateChanged += OnSessionStateChanged;
            CurrentUnitySession.Deleted += OnSessionDeleted;
            CurrentUnitySession.PlayerJoined += OnPlayerJoined;
            CurrentUnitySession.PlayerHasLeft += OnPlayerHasLeft;
            CurrentUnitySession.RemovedFromSession += OnRemovedFromSession;
            CurrentUnitySession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;
            CurrentUnitySession.SessionPropertiesChanged += OnSessionPropertiesChanged;
        }

        void UnsubscribeFromJoinedSession()
        {
            CurrentUnitySession.Changed -= OnSessionChanged;
            CurrentUnitySession.StateChanged -= OnSessionStateChanged;
            CurrentUnitySession.Deleted -= OnSessionDeleted;
            CurrentUnitySession.PlayerJoined -= OnPlayerJoined;
            CurrentUnitySession.PlayerHasLeft -= OnPlayerHasLeft;
            CurrentUnitySession.RemovedFromSession -= OnRemovedFromSession;
            CurrentUnitySession.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
            CurrentUnitySession.SessionPropertiesChanged -= OnSessionPropertiesChanged;
        }

        void OnSessionChanged()
        {
            _localSession.ApplyRemoteData(CurrentUnitySession);

            // as client, check if host is still in session
            if (!_localUser.IsHost)
            {
                foreach (var sessionUser in _localSession.SessionUsers)
                {
                    if (sessionUser.Value.IsHost)
                    {
                        return;
                    }
                }

                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Host left the session", "Disconnecting.", UnityServiceErrorMessage.Service.Session));
                EndTracking();

                // no need to disconnect Netcode, it should already be handled by Netcode's callback to disconnect
            }
        }

        void OnSessionStateChanged(SessionState sessionState)
        {
            switch (sessionState)
            {
                case SessionState.None:
                    break;
                case SessionState.Connected:
                    Logger.Log("Session state changed: Session connected.");
                    break;
                case SessionState.Disconnected:
                    Logger.Log("Session state changed: Session disconnected.");
                    break;
                case SessionState.Deleted:
                    Logger.Log("Session state changed: Session deleted.");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sessionState), sessionState, null);
            }
        }

        void OnSessionDeleted()
        {
            Logger.Log("Session deleted.");
            ResetSession();
            EndTracking();
        }

        void OnPlayerJoined(string playerId)
        {
            Logger.Log($"Player joined: {playerId}");
        }

        void OnPlayerHasLeft(string playerId)
        {
            Logger.Log($"Player has left: {playerId}");
        }

        void OnRemovedFromSession()
        {
            Logger.Log("Removed from Session.");
            ResetSession();
            EndTracking();
        }

        void OnPlayerPropertiesChanged()
        {
            Logger.Log("Player properties changed.");
        }

        void OnSessionPropertiesChanged()
        {
            Logger.Log("Session properties changed.");
        }

        /// <summary>
        /// Used for getting the list of all active sessions, without needing full info for each.
        /// </summary>
        public async Task RetrieveAndPublishSessionListAsync()
        {
            if (!_rateLimitQuery.CanCall)
            {
                Logger.LogWarning("Retrieving the session list hit the rate limit. Will try again soon...");
                return;
            }

            try
            {
                var queryResults = await _multiplayerServicesInterface.QuerySessions();
                EventBus<SessionListFetchedMessage>.Raise(new SessionListFetchedMessage(queryResults.Sessions));
            }
            catch (Exception e)
            {
                PublishError(e);
            }
        }

        public async Task<ISession> ReconnectToSessionAsync()
        {
            try
            {
                return await _multiplayerServicesInterface.ReconnectToSession(_localSession.SessionID);
            }
            catch (Exception e)
            {
                PublishError(e, true);
            }

            return null;
        }

        /// <summary>
        /// Attempt to leave a session
        /// </summary>
        async void LeaveSessionAsync()
        {
            try
            {
                await CurrentUnitySession.LeaveAsync();
            }
            catch (Exception e)
            {
                PublishError(e, true);
            }
            finally
            {
                ResetSession();
            }
        }

        public async void RemovePlayerFromSessionAsync(string uasId)
        {
            if (_localUser.IsHost)
            {
                try
                {
                    await CurrentUnitySession.AsHost().RemovePlayerAsync(uasId);
                }
                catch (Exception e)
                {
                    PublishError(e);
                }
            }
            else
            {
                Debug.LogError("Only the host can remove other players from the session.");
            }
        }

        async void DeleteSessionAsync()
        {
            if (_localUser.IsHost)
            {
                try
                {
                    await CurrentUnitySession.AsHost().DeleteAsync();
                }
                catch (Exception e)
                {
                    PublishError(e);
                }
                finally
                {
                    ResetSession();
                }
            }
            else
            {
                Debug.LogError("Only the host can delete a session.");
            }
        }

        void PublishError(Exception e, bool checkIfDeleted = false)
        {
            if (e is not AggregateException aggregateException)
            {
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Session Error", e.Message, UnityServiceErrorMessage.Service.Session, e));
                return;
            }

            if (aggregateException.InnerException is not SessionException sessionException)
            {
                EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Session Error", e.Message, UnityServiceErrorMessage.Service.Session, e));
                return;
            }

            // If session is not found and if we are not the host, it has already been deleted. No need to publish the error here.
            if (checkIfDeleted)
            {
                if (sessionException.Error == SessionError.SessionNotFound && !_localUser.IsHost)
                {
                    return;
                }
            }

            if (sessionException.Error == SessionError.RateLimitExceeded)
            {
                _rateLimitJoin.PutOnCooldown();
                return;
            }

            var reason = e.InnerException == null ? e.Message : $"{e.Message} ({e.InnerException.Message})"; // Session error type, then HTTP error type.
            EventBus<UnityServiceErrorMessage>.Raise(new UnityServiceErrorMessage("Session Error", reason, UnityServiceErrorMessage.Service.Session, e));
        }
    }
}
