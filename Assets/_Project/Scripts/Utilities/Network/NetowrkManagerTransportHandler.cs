using Sirenix.OdinInspector;
using Unity.Netcode;
using Unity.Netcode.Transports.SinglePlayer;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Zone8.Utilities.Network
{
    public class NetowrkManagerTransportHandler : MonoBehaviour
    {
        public enum TransportType
        {
            SinglePlayer,
            Relay,
            Distriputed
        }

        [SerializeField] bool _autoSetup = true;
        [ShowIf("_autoSetup")]
        [SerializeField] private TransportType _transportType = TransportType.SinglePlayer;

        private NetworkManager _networkManager => NetworkManager.Singleton;

        private void Start()
        {
            if (_autoSetup)
            {
                Setup(_transportType);
            }
        }

        public void Setup(TransportType transportType)
        {
            switch (transportType)
            {
                case TransportType.SinglePlayer:
                    if (!_networkManager.gameObject.TryGetComponent(out SinglePlayerTransport singlePlayerTransport))
                    {
                        _networkManager.transform.gameObject.AddComponent<SinglePlayerTransport>();
                    }
                    _networkManager.NetworkConfig.NetworkTransport = _networkManager.gameObject.GetComponent<SinglePlayerTransport>();
                    _networkManager.StartHost();
                    break;
                case TransportType.Relay:
                case TransportType.Distriputed:
                    if (!_networkManager.gameObject.TryGetComponent(out UnityTransport unityTransport))
                    {
                        _networkManager.transform.gameObject.AddComponent<UnityTransport>();
                        _networkManager.NetworkConfig.NetworkTransport = _networkManager.gameObject.GetComponent<UnityTransport>();
                    }
                    break;

                default:
                    Debug.LogError("Invalid transport type");
                    break;
            }
        }
    }
}
