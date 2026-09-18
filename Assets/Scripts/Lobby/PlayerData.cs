using Unity.Netcode;
using Unity.Collections;
using System;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public FixedString32Bytes PlayerName;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
    }

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId &&
               PlayerName.Equals(other.PlayerName);
    }
}
