using UnityEngine;
using Zone8.Events;
using Zone8.UnityServices.Sessions;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a connected client. When being disconnected, transitions to the
    /// ClientReconnecting state if no reason is given, or to the Offline state.
    /// </summary>
    internal class ClientConnectedState<T> : OnlineState<T> where T : struct, ISessionPlayerData
    {
        private MultiplayerServicesFacade _multiplayerServicesFacade;


        public ClientConnectedState(ConnectionManager<T> connectionManager) : base(connectionManager)
        {
            _multiplayerServicesFacade = MultiplayerServicesFacade.Instance;
        }

        public override void Enter()
        {
            if (_multiplayerServicesFacade.CurrentUnitySession != null)
            {
                _multiplayerServicesFacade.BeginTracking();
            }
        }

        public override void Exit() { }

        public override void OnClientDisconnect(ulong _)
        {
            var disconnectReason = _connectionManager.NetworkManager.DisconnectReason;
            if (string.IsNullOrEmpty(disconnectReason) ||
                disconnectReason == "Disconnected due to host shutting down.")
            {
                EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(ConnectStatus.Reconnecting));
                _connectionManager.ChangeState(_connectionManager._clientReconnecting);
            }
            else
            {
                var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(connectStatus));
                _connectionManager.ChangeState(_connectionManager._offline);

            }
        }
    }
}
