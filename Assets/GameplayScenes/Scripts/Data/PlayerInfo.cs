using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

//this struct contains the basic info of a player
public struct PlayerInfo : INetworkSerializable, IEquatable<PlayerInfo>
{
    public FixedString128Bytes name;
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

    public override bool Equals(object otherObj)
    {
        PlayerInfo other = (PlayerInfo)otherObj;
        if(name.Equals(other.name)&&id==other.id) 
            return true;
        return false;
    }

    public bool Equals(PlayerInfo other)
    {
        if (name.Equals(other.name) && id == other.id)
            return true;
        return false;
    }
}
