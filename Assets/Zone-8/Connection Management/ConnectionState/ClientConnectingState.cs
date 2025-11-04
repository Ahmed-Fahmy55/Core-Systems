using Zone8.Events;
using System;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to when a client is attempting to connect to a server. Starts the client when
    /// entering. If successful, transitions to the ClientConnected state. If not, transitions to the Offline state.
    /// </summary>
    class ClientConnectingState : OnlineState
    {
        protected ConnectionMethodBase _connectionMethod;

        public ClientConnectingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public ClientConnectingState Configure(ConnectionMethodBase baseConnectionMethod)
        {
            _connectionMethod = baseConnectionMethod;
            return this;
        }

        public override void Enter()
        {
            ConnectClientAsync();
        }

        public override void Exit() { }

        public override void OnClientConnected(ulong _)
        {
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(ConnectStatus.Success));
            _connectionManager.ChangeState(_connectionManager._clientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            // client ID is for sure ours here
            StartingClientFailed();
        }

        void StartingClientFailed()
        {
            var disconnectReason = _connectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason))
            {
                EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(ConnectStatus.StartClientFailed));
                _connectionManager.ChangeState(_connectionManager._clientReconnecting);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(connectStatus));
                _connectionManager.ChangeState(_connectionManager._offline);
            }
        }

        internal void ConnectClientAsync()
        {
            try
            {
                _connectionMethod.SetupClientConnection();

                if (_connectionMethod is ConnectionMethodIP)
                {
                    if (!_connectionManager.NetworkManager.StartClient())
                    {
                        throw new Exception("NetworkManager StartClient failed");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Error connecting client, see following exception");
                Logger.LogError(e);
                StartingClientFailed();
                throw;
            }
        }
    }
}
