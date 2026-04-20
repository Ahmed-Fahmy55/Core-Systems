using Zone8.Events;
using System;
using Unity.Netcode;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a host starting up. Starts the host when entering the state. If successful,
    /// transitions to the Hosting state, if not, transitions back to the Offline state.
    /// </summary>
    class StartingHostState : OnlineState
    {
        ConnectionMethodBase _connectionMethod;

        public StartingHostState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public StartingHostState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            StartHost();
        }

        public override void Exit() { }

        public override void OnServerStarted()
        {
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.Success });
            _connectionManager.ChangeState(_connectionManager._hosting);
        }

        public override void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            var connectionData = request.Payload;
            var clientId = request.ClientNetworkId;

            // This happens when starting as a host, before the end of the StartHost call. In that case, we simply approve ourselves.
            if (clientId == _connectionManager.NetworkManager.LocalClientId)
            {
                var payload = System.Text.Encoding.UTF8.GetString(connectionData);
                var connectionPayload = JsonUtility.FromJson<ConnectionPayload>(payload);

                SessionManager<SessionPlayerData>.Instance.SetupConnectingPlayerSessionData(clientId, connectionPayload.playerId,
                    new SessionPlayerData(clientId, true));
                response.Approved = true;
            }
        }

        public override void OnServerStopped()
        {
            StartHostFailed();
        }

        void StartHost()
        {
            try
            {
                _connectionMethod.SetupHostConnection();

                if (_connectionMethod is ConnectionMethodIP)
                {
                    // NGO's StartHost launches everything
                    if (!_connectionManager.NetworkManager.StartHost())
                    {
                        StartHostFailed();
                    }
                }
            }
            catch (Exception)
            {
                StartHostFailed();
                throw;
            }
        }

        void StartHostFailed()
        {
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.StartHostFailed });
            _connectionManager.ChangeState(_connectionManager._offline);
        }
    }
}
