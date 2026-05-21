using System.Text.RegularExpressions;
using Unity.Networking.Transport;
using UnityEngine;
using Zone8.Events;

namespace Zone8.Multiplayer.ConnectionManagement.UI
{
    public class IPUIMediator : MonoBehaviour
    {
        public const string k_DefaultIP = "127.0.0.1";
        public const int k_DefaultPort = 7777;

        [SerializeField] GameObject _signInSpinner;
        [SerializeField] IPConnectionWindow _ipConnectionWindow;


        //ConnectionManager _connectionManager;
        EventBinding<ConnectionMessageEvent> _connectStatusSubscriber;


        void Awake()
        {
            //_connectionManager = _connectionManager = FindAnyObjectByType<ConnectionManager>();
            _connectStatusSubscriber = new EventBinding<ConnectionMessageEvent>(OnConnectStatusMessage);
            EventBus<ConnectionMessageEvent>.Register(_connectStatusSubscriber);
        }

        void OnDestroy()
        {
            EventBus<ConnectionMessageEvent>.Deregister(_connectStatusSubscriber);
        }

        public void HostIPRequest(string ip, string port)
        {
            int.TryParse(port, out var portNum);
            if (portNum <= 0)
            {
                portNum = k_DefaultPort;
            }

            ip = string.IsNullOrEmpty(ip) ? k_DefaultIP : ip;

            _signInSpinner.SetActive(true);
            // _connectionManager.StartHostIp(ip, portNum);
        }

        public void JoinWithIP(string ip, string port)
        {
            int.TryParse(port, out var portNum);
            if (portNum <= 0)
            {
                portNum = k_DefaultPort;
            }

            ip = string.IsNullOrEmpty(ip) ? k_DefaultIP : ip;

            _signInSpinner.SetActive(true);

            // _connectionManager.StartClientIp(ip, portNum);

            _ipConnectionWindow.ShowConnectingWindow();
        }

        public void JoiningWindowCancelled()
        {
            DisableSignInSpinner();
            RequestShutdown();
        }

        public void DisableSignInSpinner()
        {
            _signInSpinner.SetActive(false);
        }

        void RequestShutdown()
        {
            /*            if (_connectionManager && _connectionManager.NetworkManager)
                        {
                            _connectionManager.RequestShutdown();
                        }*/
        }

        // To be called from the Cancel (X) UI button
        public void CancelConnectingWindow()
        {
            RequestShutdown();
            _ipConnectionWindow.CancelConnectionWindow();
        }

        /// <summary>
        /// Sanitize user IP address InputField box allowing only numbers and '.'. This also prevents undesirable
        /// invisible characters from being copy-pasted accidentally.
        /// </summary>
        /// <param name="dirtyString"> string to sanitize. </param>
        /// <returns> Sanitized text string. </returns>
        public static string SanitizeIP(string dirtyString)
        {
            return Regex.Replace(dirtyString, "[^0-9.]", "");
        }

        /// <summary>
        /// Sanitize user port InputField box allowing only numbers. This also prevents undesirable invisible characters
        /// from being copy-pasted accidentally.
        /// </summary>
        /// <param name="dirtyString"> string to sanitize. </param>
        /// <returns> Sanitized text string. </returns>
        public static string SanitizePort(string dirtyString)
        {

            return Regex.Replace(dirtyString, "[^0-9]", "");
        }

        public static bool AreIpAddressAndPortValid(string ipAddress, string port)
        {
            var portValid = ushort.TryParse(port, out var portNum);
            return portValid && NetworkEndpoint.TryParse(ipAddress, portNum, out var networkEndPoint);
        }

        void OnConnectStatusMessage(ConnectionMessageEvent connectStatus)
        {
            DisableSignInSpinner();
        }
    }
}
