using System;
using Unity.Netcode;
using UnityEngine;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement
{
    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
    }

    public class ConnectionManager<T> : MonoBehaviour where T : struct, ISessionPlayerData
    {
        private ConnectionState<T> _currentState;
        private NetworkManager _networkManager;

        public NetworkManager NetworkManager => _networkManager;

        [SerializeField]
        private int _reconnectAttemptsNumb = 2;

        public int ReconnectAttemptsNumb => _reconnectAttemptsNumb;
        public int MaxConnectedPlayers = 8;

        internal OfflineState<T> _offline;
        internal ClientConnectingState<T> _clientConnecting;
        internal ClientConnectedState<T> _clientConnected;
        internal ClientReconnectingState<T> _clientReconnecting;
        internal StartingHostState<T> _startingHost;
        internal HostingState<T> _hosting;

        private void Start()
        {
            SetupStates();
            _networkManager = NetworkManager.Singleton;
            _currentState = _offline;

            NetworkManager.OnConnectionEvent += OnConnectionEvent;
            NetworkManager.OnServerStarted += OnServerStarted;
            NetworkManager.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.OnTransportFailure += OnTransportFailure;
            NetworkManager.OnServerStopped += OnServerStopped;
        }

        private void OnDestroy()
        {
            if (NetworkManager != null)
            {
                NetworkManager.OnConnectionEvent -= OnConnectionEvent;
                NetworkManager.OnServerStarted -= OnServerStarted;
                NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
                NetworkManager.OnTransportFailure -= OnTransportFailure;
                NetworkManager.OnServerStopped -= OnServerStopped;
            }
        }

        private void SetupStates()
        {
            _offline = new OfflineState<T>(MultiplayerServicesFacade.Instance, this);
            _clientConnected = new ClientConnectedState<T>(this);
            _clientConnecting = new ClientConnectingState<T>(this);
            _clientReconnecting = new ClientReconnectingState<T>(this);
            _startingHost = new StartingHostState<T>(this);
            _hosting = new HostingState<T>(this);
        }

        internal void ChangeState(ConnectionState<T> nextState)
        {
            if (_currentState != null)
            {
                _currentState.Exit();
            }
            _currentState = nextState;
            _currentState.Enter();
        }

        // ... (Remaining methods updated to use ConnectionState<T> where necessary)
        private void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            switch (connectionEventData.EventType)
            {
                case Unity.Netcode.ConnectionEvent.ClientConnected:
                    _currentState.OnClientConnected(connectionEventData.ClientId);
                    break;
                case Unity.Netcode.ConnectionEvent.ClientDisconnected:
                    _currentState.OnClientDisconnect(connectionEventData.ClientId);
                    break;
            }
        }

        private void OnServerStarted() => _currentState.OnServerStarted();
        private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) => _currentState.ApprovalCheck(request, response);
        private void OnTransportFailure() => _currentState.OnTransportFailure();
        private void OnServerStopped(bool _) => _currentState.OnServerStopped();
        public void StartClientSession() => _currentState.StartClientSession();
        public void StartClientIp(string ipaddress, int port) => _currentState.StartClientIP(ipaddress, port);
        public void StartHostSession() => _currentState.StartHostSession();
        public void StartHostIp(string ipaddress, int port) => _currentState.StartHostIP(ipaddress, port);
        public void RequestShutdown() => _currentState.OnUserRequestedShutdown();
    }
}