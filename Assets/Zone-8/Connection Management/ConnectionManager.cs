using System;
using Unity.Netcode;
using UnityEngine;
using Zone8.Events;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement
{
    public enum ConnectStatus
    {
        Undefined,
        Success,                  //client successfully connected. This may also be a successful reconnect.
        ServerFull,               //can't join, server is already at capacity.
        LoggedInAgain,            //logged in on a separate client, causing this one to be kicked out.
        UserRequestedDisconnect,  //Intentional Disconnect triggered by the user.
        GenericDisconnect,        //server disconnected, but no specific reason given.
        Reconnecting,             //client lost connection and is attempting to reconnect.
        IncompatibleBuildType,    //client build type is incompatible with server.
        HostEndedSession,         //host intentionally ended the session.
        StartHostFailed,          // server failed to bind
        StartClientFailed         // failed to connect to server and/or invalid network endpoint
    }

    public struct ReconnectMessageEvent : IEvent
    {
        public int CurrentAttempt;
        public int MaxAttempt;

        public ReconnectMessageEvent(int currentAttempt, int maxAttempt)
        {
            CurrentAttempt = currentAttempt;
            MaxAttempt = maxAttempt;
        }
    }

    public struct ConnectionMessageEvent : IEvent
    {
        public ConnectStatus ConnectStatus;

        public ConnectionMessageEvent(ConnectStatus status)
        {
            ConnectStatus = status;
        }
    }

    [Serializable]
    public class ConnectionPayload
    {
        public string playerId;
    }

    /// <summary>
    /// This state machine handles connection through the NetworkManager. It is responsible for listening to
    /// NetworkManger callbacks and other outside calls and redirecting them to the current ConnectionState object.
    /// </summary>
    public class ConnectionManager : MonoBehaviour
    {

        ConnectionState _currentState;
        NetworkManager _networkManager;

        public NetworkManager NetworkManager => _networkManager;

        [SerializeField]
        int _reconnectAttemptsNumb = 2;

        public int ReconnectAttemptsNumb => _reconnectAttemptsNumb;


        public int MaxConnectedPlayers = 8;

        internal OfflineState _offline;
        internal ClientConnectingState _clientConnecting;
        internal ClientConnectedState _clientConnected;
        internal ClientReconnectingState _clientReconnecting;
        internal StartingHostState _startingHost;
        internal HostingState _hosting;


        void Start()
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

        void OnDestroy()
        {
            NetworkManager.OnConnectionEvent -= OnConnectionEvent;
            NetworkManager.OnServerStarted -= OnServerStarted;
            NetworkManager.ConnectionApprovalCallback -= ApprovalCheck;
            NetworkManager.OnTransportFailure -= OnTransportFailure;
            NetworkManager.OnServerStopped -= OnServerStopped;
        }

        private void SetupStates()
        {
            _offline = new(MultiplayerServicesFacade.Instance, this);
            _clientConnected = new(this);
            _clientConnecting = new(this);
            _clientReconnecting = new(this);
            _startingHost = new(this);
            _hosting = new(this);
        }

        internal void ChangeState(ConnectionState nextState)
        {
            Logger.Log($"{name}: Changed connection state from {_currentState.GetType().Name} to {nextState.GetType().Name}.");

            if (_currentState != null)
            {
                _currentState.Exit();
            }
            _currentState = nextState;
            _currentState.Enter();
        }

        void OnConnectionEvent(NetworkManager networkManager, ConnectionEventData connectionEventData)
        {
            switch (connectionEventData.EventType)
            {
                case Unity.Netcode.ConnectionEvent.ClientConnected:
                    Logger.Log($"Client {connectionEventData.ClientId} connected.");
                    _currentState.OnClientConnected(connectionEventData.ClientId);
                    break;
                case Unity.Netcode.ConnectionEvent.ClientDisconnected:
                    _currentState.OnClientDisconnect(connectionEventData.ClientId);
                    break;
            }
        }

        void OnServerStarted()
        {
            _currentState.OnServerStarted();
        }

        void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            _currentState.ApprovalCheck(request, response);
        }

        void OnTransportFailure()
        {
            _currentState.OnTransportFailure();
        }

        void OnServerStopped(bool _) // we don't need this parameter as the ConnectionState already carries the relevant information
        {
            _currentState.OnServerStopped();
        }

        public void StartClientSession()
        {
            _currentState.StartClientSession();
        }

        public void StartClientIp(string ipaddress, int port)
        {
            _currentState.StartClientIP(ipaddress, port);
        }

        public void StartHostSession()
        {
            _currentState.StartHostSession();
        }

        public void StartHostIp(string ipaddress, int port)
        {
            _currentState.StartHostIP(ipaddress, port);
        }

        public void RequestShutdown()
        {
            _currentState.OnUserRequestedShutdown();
        }
    }
}
