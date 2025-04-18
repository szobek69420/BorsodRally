using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public struct CarOrientation : INetworkSerializable
{
    public const int CAR_NOT_USED = int.MaxValue;

    public int id;

    //car orientation
    public float carPosX, carPosY, carPosZ;
    public float carRotX, carRotY, carRotZ;

    //wheel orientations
    public float wheelFlPosX, wheelFlPosY, wheelFlPosZ;
    public float wheelFlRotX, wheelFlRotY, wheelFlRotZ;

    public float wheelFrPosX, wheelFrPosY, wheelFrPosZ;
    public float wheelFrRotX, wheelFrRotY, wheelFrRotZ;

    public float wheelRlPosX, wheelRlPosY, wheelRlPosZ;
    public float wheelRlRotX, wheelRlRotY, wheelRlRotZ;

    public float wheelRrPosX, wheelRrPosY, wheelRrPosZ;
    public float wheelRrRotX, wheelRrRotY, wheelRrRotZ;

    //car velocity
    public float velocityX, velocityY, velocityZ;

    public CarOrientation(int placeHolder)//use this if the car is not in use
    {
        id = CAR_NOT_USED;

        carPosX = 0; carPosY = 0; carPosZ = 0;
        carRotX = 0; carRotY = 0; carRotZ = 0;

        wheelFlPosX = 0; wheelFlPosY = 0; wheelFlPosZ = 0;
        wheelFlRotX = 0; wheelFlRotY = 0; wheelFlRotZ = 0;

        wheelFrPosX = 0; wheelFrPosY = 0; wheelFrPosZ = 0;
        wheelFrRotX = 0; wheelFrRotY = 0; wheelFrRotZ = 0;

        wheelRlPosX = 0; wheelRlPosY = 0; wheelRlPosZ = 0;
        wheelRlRotX = 0; wheelRlRotY = 0; wheelRlRotZ = 0;

        wheelRrPosX = 0; wheelRrPosY = 0; wheelRrPosZ = 0;
        wheelRrRotX = 0; wheelRrRotY = 0; wheelRrRotZ = 0;

        velocityX = 0; velocityY = 0; velocityZ = 0;
    }

    public CarOrientation(int ownerId, Transform car, Transform wheelFl, Transform wheelFr, Transform wheelRl, Transform wheelRr, Vector3 velocity)
    {
        id = ownerId;

        //car
        carPosX = car.position.x;
        carPosY = car.position.y;
        carPosZ = car.position.z;

        carRotX = car.rotation.eulerAngles.x;
        carRotY = car.rotation.eulerAngles.y;
        carRotZ = car.rotation.eulerAngles.z;

        //wheels
        wheelFlPosX = wheelFl.position.x;
        wheelFlPosY = wheelFl.position.y;
        wheelFlPosZ = wheelFl.position.z;

        wheelFlRotX = wheelFl.rotation.eulerAngles.x;
        wheelFlRotY = wheelFl.rotation.eulerAngles.y;
        wheelFlRotZ = wheelFl.rotation.eulerAngles.z;


        wheelFrPosX = wheelFr.position.x;
        wheelFrPosY = wheelFr.position.y;
        wheelFrPosZ = wheelFr.position.z;

        wheelFrRotX = wheelFr.rotation.eulerAngles.x;
        wheelFrRotY = wheelFr.rotation.eulerAngles.y;
        wheelFrRotZ = wheelFr.rotation.eulerAngles.z;


        wheelRlPosX = wheelRl.position.x;
        wheelRlPosY = wheelRl.position.y;
        wheelRlPosZ = wheelRl.position.z;

        wheelRlRotX = wheelRl.rotation.eulerAngles.x;
        wheelRlRotY = wheelRl.rotation.eulerAngles.y;
        wheelRlRotZ = wheelRl.rotation.eulerAngles.z;


        wheelRrPosX = wheelRr.position.x;
        wheelRrPosY = wheelRr.position.y;
        wheelRrPosZ = wheelRr.position.z;

        wheelRrRotX = wheelRr.rotation.eulerAngles.x;
        wheelRrRotY = wheelRr.rotation.eulerAngles.y;
        wheelRrRotZ = wheelRr.rotation.eulerAngles.z;

        //velocity
        velocityX = velocity.x;
        velocityY = velocity.y;
        velocityZ = velocity.z;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref id);


        serializer.SerializeValue(ref carPosX);
        serializer.SerializeValue(ref carPosY);
        serializer.SerializeValue(ref carPosZ);
        serializer.SerializeValue(ref carRotX);
        serializer.SerializeValue(ref carRotY);
        serializer.SerializeValue(ref carRotZ);


        serializer.SerializeValue(ref wheelFlPosX);
        serializer.SerializeValue(ref wheelFlPosY);
        serializer.SerializeValue(ref wheelFlPosZ);
        serializer.SerializeValue(ref wheelFlRotX);
        serializer.SerializeValue(ref wheelFlRotY);
        serializer.SerializeValue(ref wheelFlRotZ);

        serializer.SerializeValue(ref wheelFrPosX);
        serializer.SerializeValue(ref wheelFrPosY);
        serializer.SerializeValue(ref wheelFrPosZ);
        serializer.SerializeValue(ref wheelFrRotX);
        serializer.SerializeValue(ref wheelFrRotY);
        serializer.SerializeValue(ref wheelFrRotZ);

        serializer.SerializeValue(ref wheelRlPosX);
        serializer.SerializeValue(ref wheelRlPosY);
        serializer.SerializeValue(ref wheelRlPosZ);
        serializer.SerializeValue(ref wheelRlRotX);
        serializer.SerializeValue(ref wheelRlRotY);
        serializer.SerializeValue(ref wheelRlRotZ);

        serializer.SerializeValue(ref wheelRrPosX);
        serializer.SerializeValue(ref wheelRrPosY);
        serializer.SerializeValue(ref wheelRrPosZ);
        serializer.SerializeValue(ref wheelRrRotX);
        serializer.SerializeValue(ref wheelRrRotY);
        serializer.SerializeValue(ref wheelRrRotZ);

        serializer.SerializeValue(ref velocityX);
        serializer.SerializeValue(ref velocityY);
        serializer.SerializeValue(ref velocityZ);
    }
}
