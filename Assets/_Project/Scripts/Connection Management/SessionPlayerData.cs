using Unity.Netcode;
using Zone8.Utilities.Network;

namespace Zone8.Multiplayer.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData, INetworkSerializable
    {
        public bool IsConnected
        {
            get => _isConnected;
            set => _isConnected = value;
        }

        public ulong ClientID
        {
            get => _clientID;
            set => _clientID = value;
        }

        public NetworkString PlayerName;

        public int PlayerIndex;
        private bool _isConnected;
        private ulong _clientID;

        public SessionPlayerData(ulong clientID, bool isConnected)
        {
            _isConnected = isConnected;
            _clientID = clientID;
            PlayerName = "";
            PlayerIndex = -1;
        }



        public void Reinitialize()
        {
            PlayerName = "";
            PlayerIndex = -1;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _isConnected);
            serializer.SerializeValue(ref _clientID);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref PlayerIndex);
        }
    }
}
