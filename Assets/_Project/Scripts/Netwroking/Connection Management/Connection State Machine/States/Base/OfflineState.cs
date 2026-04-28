using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to when the NetworkManager is shut down. From this state we can transition to the
    /// ClientConnecting sate, if starting as a client, or the StartingHost state, if starting as a host.
    /// </summary>
    class OfflineState<T> : ConnectionState<T> where T : struct, ISessionPlayerData
    {
        private MultiplayerServicesFacade _multiplayerServicesFacade;


        public OfflineState(MultiplayerServicesFacade multiplayerServicesFacade, ConnectionManager<T> connectionManager) : base(connectionManager)
        {
            _multiplayerServicesFacade = multiplayerServicesFacade;
        }

        public override void Enter()
        {
            _multiplayerServicesFacade.EndTracking();

            if (_connectionManager.NetworkManager.IsConnectedClient)
                _connectionManager.NetworkManager.Shutdown();
        }

        public override void Exit() { }

        public override void StartClientIP(string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port);
            _connectionManager._clientReconnecting.Configure(connectionMethod);
            _connectionManager.ChangeState(_connectionManager._clientConnecting.Configure(connectionMethod));
        }

        public override void StartClientSession()
        {
            var connectionMethod = new ConnectionMethodRelay();
            _connectionManager._clientReconnecting.Configure(connectionMethod);
            _connectionManager.ChangeState(_connectionManager._clientConnecting.Configure(connectionMethod));
        }

        public override void StartHostIP(string ipaddress, int port)
        {
            var connectionMethod = new ConnectionMethodIP(ipaddress, (ushort)port);
            _connectionManager.ChangeState(_connectionManager._startingHost.Configure(connectionMethod));
        }

        public override void StartHostSession()
        {
            var connectionMethod = new ConnectionMethodRelay();
            _connectionManager.ChangeState(_connectionManager._startingHost.Configure(connectionMethod));
        }
    }
}
