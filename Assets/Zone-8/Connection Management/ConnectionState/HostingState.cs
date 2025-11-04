using Unity.Netcode;
using UnityEngine;
using Zone8.Events;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a listening host. Handles incoming client connections. When shutting down or
    /// being timed out, transitions to the Offline state.
    /// </summary>
    internal class HostingState : OnlineState
    {
        private MultiplayerServicesFacade _multiplayerServicesFacade;

        // used in ApprovalCheck. This is intended as a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
        private const int k_MaxConnectPayload = 1024;

        public HostingState(ConnectionManager connectionManager) : base(connectionManager)
        {
            _multiplayerServicesFacade = MultiplayerServicesFacade.Instance;
        }

        public override void Enter()
        {
            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();

            if (_multiplayerServicesFacade.CurrentUnitySession != null)
            {
                _multiplayerServicesFacade.BeginTracking();
            }
        }

        public override void Exit()
        {
            SessionManager<SessionPlayerData>.Instance.OnServerEnded();
        }

        public override void OnClientConnected(ulong clientId)
        {
            var playerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (playerData != null)
            {
                EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.Success });
            }
            else
            {
                // This should not happen since player data is assigned during connection approval
                Logger.LogError($"No player data associated with client {clientId}");
                var reason = JsonUtility.ToJson(ConnectStatus.GenericDisconnect);
                _connectionManager.NetworkManager.DisconnectClient(clientId, reason);
            }
        }

        public override void OnClientDisconnect(ulong clientId)
        {
            if (clientId != _connectionManager.NetworkManager.LocalClientId)
            {
                var playerId = SessionManager<SessionPlayerData>.Instance.GetPlayerId(clientId);
                if (playerId != null)
                {
                    var sessionData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(playerId);
                    if (sessionData.HasValue)
                    {
                        EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.GenericDisconnect });
                    }
                    SessionManager<SessionPlayerData>.Instance.DisconnectClient(clientId);
                }
            }
        }

        public override void OnUserRequestedShutdown()
        {
            var reason = JsonUtility.ToJson(ConnectStatus.HostEndedSession);
            for (var i = _connectionManager.NetworkManager.ConnectedClientsIds.Count - 1; i >= 0; i--)
            {
                var id = _connectionManager.NetworkManager.ConnectedClientsIds[i];
                if (id != _connectionManager.NetworkManager.LocalClientId)
                {
                    _connectionManager.NetworkManager.DisconnectClient(id, reason);
                }
            }

            _connectionManager.ChangeState(_connectionManager._offline);
        }

        public override void OnServerStopped()
        {
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.GenericDisconnect });
            _connectionManager.ChangeState(_connectionManager._offline);
        }

        /// <summary>
        /// This logic plugs into the "ConnectionApprovalResponse" exposed by Netcode.NetworkManager. It is run every time a client connects to us.
        /// The complementary logic that runs when the client starts its connection can be found in ClientConnectingState.
        /// </summary>
        /// <remarks>
        /// Multiple things can be done here, some asynchronously. For example, it could authenticate your user against an auth service like UGS' auth service. It can
        /// also send custom messages to connecting users before they receive their connection result (this is useful to set status messages client side
        /// when connection is refused, for example).
        /// Note on authentication: It's usually harder to justify having authentication in a client hosted game's connection approval. Since the host can't be trusted,
        /// clients shouldn't send it private authentication tokens you'd usually send to a dedicated server.
        /// </remarks>
        /// <param name="request"> The initial request contains, among other things, binary data passed into StartClient. In our case, this is the client's GUID,
        /// which is a unique identifier for their install of the game that persists across app restarts.
        ///  <param name="response"> Our response to the approval process. In case of connection refusal with custom return message, we delay using the Pending field.
        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Debug.Log("ApprovalCheck invoked for client " + request.ClientNetworkId);
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;
            if (connectionData.Length > k_MaxConnectPayload)
            {
                // If connectionData too high, deny immediately to avoid wasting time on the server. This is intended as
                // a bit of light protection against DOS attacks that rely on sending silly big buffers of garbage.
                response.Approved = false;
                return;
            }

            var payload = System.Text.Encoding.UTF8.GetString(connectionData);
            var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);
            var gameReturnStatus = GetConnectStatus(connectionPayload);

            if (gameReturnStatus == ConnectStatus.Success)
            {
                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, true));

                response.Approved = true;
                return;
            }
            Debug.Log($"Connection from client {clientId} denied: {gameReturnStatus}");
            response.Approved = false;
            response.Reason = JsonUtility.ToJson(gameReturnStatus);
            if (_multiplayerServicesFacade.CurrentUnitySession != null)
            {
                _multiplayerServicesFacade.RemovePlayerFromSessionAsync(connectionPayload.playerId);
            }
        }

        private ConnectStatus GetConnectStatus(ConnectionPayload connectionPayload)
        {
            if (_connectionManager.NetworkManager.ConnectedClientsIds.Count >= _connectionManager.MaxConnectedPlayers)
            {
                return ConnectStatus.ServerFull;
            }

            return SessionManager<SessionPlayerData>.Instance.IsDuplicateConnection(connectionPayload.playerId) ?
                ConnectStatus.LoggedInAgain : ConnectStatus.Success;
        }
    }
}
