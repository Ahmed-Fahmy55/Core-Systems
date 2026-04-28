using Zone8.Events;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Base class representing an online connection state.
    /// </summary>
    internal abstract class OnlineState<T> : ConnectionState<T> where T : struct, ISessionPlayerData
    {
        protected OnlineState(ConnectionManager<T> connectionManager) : base(connectionManager)
        {
        }

        public override void OnUserRequestedShutdown()
        {
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.UserRequestedDisconnect });
            _connectionManager.ChangeState(_connectionManager._offline);
        }

        public override void OnTransportFailure()
        {
            _connectionManager.ChangeState(_connectionManager._offline);
        }
    }
}
