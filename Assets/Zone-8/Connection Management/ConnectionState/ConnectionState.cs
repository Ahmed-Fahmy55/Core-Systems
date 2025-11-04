using Unity.Netcode;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Base class representing a connection state.
    /// </summary>
    abstract class ConnectionState
    {
        protected ConnectionManager _connectionManager;


        public ConnectionState(ConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }

        public abstract void Enter();

        public abstract void Exit();

        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnect(ulong clientId) { }

        public virtual void OnServerStarted() { }

        public virtual void StartClientIP(string ipaddress, int port) { }

        public virtual void StartClientSession() { }

        public virtual void StartHostIP(string ipaddress, int port) { }

        public virtual void StartHostSession() { }

        public virtual void OnUserRequestedShutdown() { }

        public virtual void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response) { }

        public virtual void OnTransportFailure() { }

        public virtual void OnServerStopped() { }
    }
}
