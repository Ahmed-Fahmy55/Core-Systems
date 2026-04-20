using Zone8.Events;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Base class representing an online connection state.
    /// </summary>
    abstract class OnlineState : ConnectionState
    {
        protected OnlineState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void OnUserRequestedShutdown()
        {
            // This behaviour will be the same for every online state
            EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent() { ConnectStatus = ConnectStatus.UserRequestedDisconnect });
            _connectionManager.ChangeState(_connectionManager._offline);
        }

        public override void OnTransportFailure()
        {
            // This behaviour will be the same for every online state
            _connectionManager.ChangeState(_connectionManager._offline);
        }
    }
}
