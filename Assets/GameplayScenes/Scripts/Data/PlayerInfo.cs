using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

//this struct contains the basic info of a player
public struct PlayerInfo : INetworkSerializable
{
    public string name;
    public int id;//it is the process id

    public PlayerInfo(string name, int id)
    {
        this.name = name;
        this.id = id;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref id);
    }
}
