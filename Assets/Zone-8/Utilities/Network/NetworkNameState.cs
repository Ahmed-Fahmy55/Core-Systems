using Unity.Collections;
using Unity.Netcode;
using UnityEngine;


namespace Zone8.Utilities.Network
{
    /// <summary>
    /// NetworkBehaviour containing only one NetworkVariableString which represents this object's name.
    /// </summary>
    public class NetworkNameState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<NetworkString> Name = new NetworkVariable<NetworkString>();
    }

    /// <summary>
    /// Wrapping FixedString so that if we want to change name max size in the future, we only do it once here
    /// </summary>
    public struct NetworkString : INetworkSerializable
    {
        FixedString32Bytes _name;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _name);
        }

        public override string ToString()
        {
            return _name.Value.ToString();
        }

        public static implicit operator string(NetworkString s) => s.ToString();
        public static implicit operator NetworkString(string s) => new NetworkString() { _name = new FixedString32Bytes(s) };
    }
}