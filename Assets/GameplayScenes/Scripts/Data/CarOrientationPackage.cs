using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct CarOrientationPackage : INetworkSerializable
{
    public int carCount;
    public CarOrientation car1, car2, car3, car4;

    public CarOrientationPackage(CarOrientation car1)
    {
        this.carCount = 1;
        this.car1 = car1;
        this.car2 = car1;
        this.car3 = car1;
        this.car4 = car1;
    }
    public CarOrientationPackage(CarOrientation car1, CarOrientation car2)
    {
        this.carCount = 2;
        this.car1 = car1;
        this.car2 = car2;
        this.car3 = car1;
        this.car4 = car1;
    }
    public CarOrientationPackage(CarOrientation car1, CarOrientation car2, CarOrientation car3)
    {
        this.carCount = 3;
        this.car1 = car1;
        this.car2 = car2;
        this.car3 = car3;
        this.car4 = car1;
    }
    public CarOrientationPackage(CarOrientation car1, CarOrientation car2, CarOrientation car3, CarOrientation car4)
    {
        this.carCount = 4;
        this.car1 = car1;
        this.car2 = car2;
        this.car3 = car3;
        this.car4 = car4;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref carCount);
        serializer.SerializeValue(ref car1);
        serializer.SerializeValue(ref car2);
        serializer.SerializeValue(ref car3);
        serializer.SerializeValue(ref car4);
    }
}
