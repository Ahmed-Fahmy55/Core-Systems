using Zone8.Events;
using System.Collections;
using UnityEngine;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// Connection state corresponding to a client attempting to reconnect to a server. It will try to reconnect a
    /// number of times defined by the ConnectionManager's NbReconnectAttempts property. If it succeeds, it will
    /// transition to the ClientConnected state. If not, it will transition to the Offline state. If given a disconnect
    /// reason first, depending on the reason given, may not try to reconnect again and transition directly to the
    /// Offline state.
    /// </summary>
    class ClientReconnectingState : ClientConnectingState
    {

        Coroutine _reconnectCoroutine;
        int _attemptsNumb;

        const float k_TimeBeforeFirstAttempt = 1;
        const float k_TimeBetweenAttempts = 5;

        public ClientReconnectingState(ConnectionManager connectionManager) : base(connectionManager)
        {
        }

        public override void Enter()
        {
            _attemptsNumb = 0;
            _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (_reconnectCoroutine != null)
            {
                _connectionManager.StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }
            EventBus<ReconnectMessageEvent>.Raise(new ReconnectMessageEvent(_connectionManager.ReconnectAttemptsNumb, _connectionManager.ReconnectAttemptsNumb));
        }

        public override void OnClientConnected(ulong _)
        {
            _connectionManager.ChangeState(_connectionManager._clientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            var disconnectReason = _connectionManager.NetworkManager.DisconnectReason;
            if (_attemptsNumb < _connectionManager.ReconnectAttemptsNumb)
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(connectStatus));
                    switch (connectStatus)
                    {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            _connectionManager.ChangeState(_connectionManager._offline);
                            break;
                        default:
                            _reconnectCoroutine = _connectionManager.StartCoroutine(ReconnectCoroutine());
                            break;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    EventBus<ConnectionMessageEvent>.Raise(new ConnectionMessageEvent(ConnectStatus.GenericDisconnect));
                }
                else
                {
                    var connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    EventBus<ConnectionMessageEvent>.Raise(new(connectStatus));
                }

                _connectionManager.ChangeState(_connectionManager._offline);
            }
        }

        IEnumerator ReconnectCoroutine()
        {
            if (_attemptsNumb > 0)
            {
                yield return new WaitForSeconds(k_TimeBetweenAttempts);
            }

            Logger.Log("Lost connection to host, trying to reconnect...");

            _connectionManager.NetworkManager.Shutdown();

            yield return new WaitWhile(() => _connectionManager.NetworkManager.ShutdownInProgress); // wait until NetworkManager completes shutting down
            Logger.Log($"Reconnecting attempt {_attemptsNumb + 1}/{_connectionManager.ReconnectAttemptsNumb}...");
            EventBus<ReconnectMessageEvent>.Raise(new(_attemptsNumb, _connectionManager.ReconnectAttemptsNumb));

            // If first attempt, wait some time before attempting to reconnect to give time to services to update
            // (i.e. if in a Session and the host shuts down unexpectedly, this will give enough time for the Session to be
            // properly deleted so that we don't reconnect to an empty Session
            if (_attemptsNumb == 0)
            {
                yield return new WaitForSeconds(k_TimeBeforeFirstAttempt);
            }

            _attemptsNumb++;
            var reconnectingSetupTask = _connectionMethod.SetupClientReconnectionAsync();
            yield return new WaitUntil(() => reconnectingSetupTask.IsCompleted);

            if (!reconnectingSetupTask.IsFaulted && reconnectingSetupTask.Result.success)
            {
                // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                ConnectClientAsync();
            }
            else
            {
                if (!reconnectingSetupTask.Result.shouldTryAgain)
                {
                    // setting number of attempts to max so no new attempts are made
                    _attemptsNumb = _connectionManager.ReconnectAttemptsNumb;
                }

                OnClientDisconnect(0);
            }
        }
    }
}
