using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Zone8.UnityServices.Sessions;
using Zone8.Utilities;

namespace Zone8.Multiplayer.ConnectionManagement
{
    /// <summary>
    /// ConnectionMethod contains all setup needed to setup NGO to be ready to start a connection, either host or client
    /// side.
    /// Please override this abstract class to add a new transport or way of connecting.
    /// </summary>
    public abstract class ConnectionMethodBase
    {
        protected NetworkManager _networkManager;
        protected ProfileManager _profileManager;

        /// <summary>
        /// Setup the host connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract void SetupHostConnection();

        /// <summary>
        /// Setup the client connection prior to starting the NetworkManager
        /// </summary>
        /// <returns></returns>
        public abstract void SetupClientConnection();

        /// <summary>
        /// Setup the client for reconnection prior to reconnecting
        /// </summary>
        /// <returns>
        /// success = true if succeeded in setting up reconnection, false if failed.
        /// shouldTryAgain = true if we should try again after failing, false if not.
        /// </returns>
        public abstract Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync();

        public ConnectionMethodBase()
        {
            _networkManager = NetworkManager.Singleton;
            _profileManager = ProfileManager.Instance;
        }

        protected void SetConnectionPayload(ConnectionPayload payload)
        {
            var payloadJson = JsonUtility.ToJson(payload);
            var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payloadJson);
            _networkManager.NetworkConfig.ConnectionData = payloadBytes;
        }

        protected string GetPlayerId()
        {

            if (Unity.Services.Core.UnityServices.State != ServicesInitializationState.Initialized)
            {
                return ClientPrefs.GetGuid() + _profileManager.Profile;
            }

            return AuthenticationService.Instance.IsSignedIn ? AuthenticationService.Instance.PlayerId : ClientPrefs.GetGuid() + _profileManager.Profile;
        }
    }

    /// <summary>
    /// Simple IP connection setup with UTP
    /// </summary>
    internal class ConnectionMethodIP : ConnectionMethodBase
    {
        private string _ipaddress;
        private ushort _port;

        public ConnectionMethodIP(string ip, ushort port) : base()
        {
            _ipaddress = ip;
            _port = port;
        }

        public override void SetupClientConnection()
        {
            SetConnectionPayload(new ConnectionPayload() { playerId = GetPlayerId() });

            var utp = (UnityTransport)_networkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipaddress, _port);
        }

        public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
        {
            // Nothing to do here
            await Awaitable.EndOfFrameAsync();
            return (true, true);
        }

        public override void SetupHostConnection()
        {
            SetConnectionPayload(new ConnectionPayload() { playerId = GetPlayerId() }); // Need to set connection payload for host as well, as host is a client too
            var utp = (UnityTransport)_networkManager.NetworkConfig.NetworkTransport;
            utp.SetConnectionData(_ipaddress, _port);
        }
    }

    /// <summary>
    /// UTP's Relay connection setup using the Session integration
    /// </summary>
    internal class ConnectionMethodRelay : ConnectionMethodBase
    {
        private MultiplayerServicesFacade _multiplayerServicesFacade;

        public ConnectionMethodRelay() : base()
        {
            _multiplayerServicesFacade = MultiplayerServicesFacade.Instance;
        }

        public override void SetupClientConnection()
        {
            SetConnectionPayload(new ConnectionPayload() { playerId = GetPlayerId() });
        }

        public override async Task<(bool success, bool shouldTryAgain)> SetupClientReconnectionAsync()
        {
            if (_multiplayerServicesFacade.CurrentUnitySession == null)
            {
                Debug.Log("Session does not exist anymore, stopping reconnection attempts.");
                return (false, false);
            }

            // When using Session with Relay, if a user is disconnected from the Relay server, the server will notify the
            // Session service and mark the user as disconnected, but will not remove them from the Session. They then have
            // some time to attempt to reconnect (defined by the "Disconnect removal time" parameter on the dashboard),
            // after which they will be removed from the Session completely.
            // See https://docs.unity.com/ugs/en-us/manual/mps-sdk/manual/join-session#Reconnect_to_a_session
            var session = await _multiplayerServicesFacade.ReconnectToSessionAsync();
            var success = session != null;
            Logger.Log(success ? "Successfully reconnected to Session." : "Failed to reconnect to Session.");
            return (success, true); // return a success if reconnecting to session returns a session
        }

        public override void SetupHostConnection()
        {
            SetConnectionPayload(new ConnectionPayload() { playerId = GetPlayerId() });
        }
    }
}
