using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct CarInput : INetworkSerializable
{
    public int id;
    public float throttleInput;
    public float brakeInput;
    public float steerInput;

    public CarInput(int id,  float throttleInput, float brakeInput, float steerInput)
    {
        this.id = id;
        this.throttleInput = throttleInput;
        this.brakeInput = brakeInput;
        this.steerInput = steerInput;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);
        serializer.SerializeValue(ref throttleInput);
        serializer.SerializeValue(ref brakeInput);
        serializer.SerializeValue(ref steerInput);

    }
}
