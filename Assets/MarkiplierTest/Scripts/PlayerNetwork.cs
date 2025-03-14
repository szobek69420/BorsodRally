using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private class InstantiatedCar
    {
        public ulong ownerId;
        public GameObject car;

        public InstantiatedCar(ulong ownerId, GameObject car)
        {
            this.ownerId = ownerId;
            this.car = car;
        }
    }

    private struct CarOrientation : INetworkSerializable
    {
        public const ulong CAR_NOT_USED = 0xffffffff;

        public ulong id;

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
        }

        public CarOrientation(ulong ownerId, Transform car, Transform wheelFl, Transform wheelFr, Transform wheelRl, Transform wheelRr)
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
        }
    }
    private struct CarOrientationPackage : INetworkSerializable
    {
        public CarOrientation car1, car2, car3, car4;
        public CarOrientationPackage(CarOrientation car1, CarOrientation car2, CarOrientation car3, CarOrientation car4)
        {
            this.car1 = car1;
            this.car2 = car2;
            this.car3 = car3;
            this.car4 = car4;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref car1);
            serializer.SerializeValue(ref car2);
            serializer.SerializeValue(ref car3);
            serializer.SerializeValue(ref car4);
        }
    }

    [SerializeField] private GameObject hostCarPrefab;
    [SerializeField] private GameObject clientCarPrefab;

    private List<InstantiatedCar> activeCars= new List<InstantiatedCar>();

    private float lastCarUpdate = 69.0f;
    private bool isConnected = false;

    public override void OnNetworkSpawn()
    {
        isConnected = true;
        SayHelloServerRpc(OwnerClientId);
    }

    // Update is called once per frame
    void Update()
    {
        if (isConnected == false)
            return;

        PilotCar();

        if(IsHost)
        {
            //send out the new positions
            lastCarUpdate += Time.deltaTime;
            if (lastCarUpdate > 0.05f)
            {
                CarOrientation[] cars = new CarOrientation[4];
                for (int i = 0; i < 4; i++)
                {
                    if (i >= activeCars.Count)//send a placeholder car orientation
                        cars[i] = new CarOrientation(69);
                    else
                    {
                        NetworkCarComponents ncc = activeCars[i].car.GetComponent<NetworkCarComponents>();
                        cars[i] = new CarOrientation(
                            activeCars[i].ownerId,
                            ncc.car,
                            ncc.wheelFl,
                            ncc.wheelFr,
                            ncc.wheelRl,
                            ncc.wheelRr
                            );
                    }
                }

                UpdateCarOrientationsClientRpc(new CarOrientationPackage(cars[0], cars[1], cars[2], cars[3]));

                lastCarUpdate = 0.0f;
            }
        }
    }

    void PilotCar()
    {
        if (IsOwner == false) return;

        float accelerationInput = Input.GetAxis("Vertical");
        float brakeInput = 0.0f;
        float steeringInput = Input.GetAxisRaw("Horizontal");

        ClientInputServerRpc(OwnerClientId, accelerationInput, brakeInput, steeringInput);
    }

    void AddCar(ulong ownerId)
    {
        if(IsHost)
        {
            GameObject car=Instantiate(hostCarPrefab);
            car.transform.position = new Vector3(0.0f, 3.0f, 0.0f);
            activeCars.Add(new InstantiatedCar(ownerId, car));
        }
        else
        {
            GameObject car = Instantiate(clientCarPrefab);
            car.transform.position = new Vector3(0.0f, 3.0f, 0.0f);
            activeCars.Add(new InstantiatedCar(ownerId, car));
        }
    }

    [ServerRpc]
    private void SayHelloServerRpc(ulong clientId)
    {
        Debug.Log("nigga");
        bool serverIsFull = activeCars.Count>=4;
        bool alreadyContained = false;
        foreach (var car in activeCars)
        {
            if (car.ownerId == clientId)
            {
                alreadyContained = true;
                break;
            }
        }

        if(!serverIsFull&&!alreadyContained)
            AddCar(clientId);
    }

    [ClientRpc]
    private void UpdateCarOrientationsClientRpc(CarOrientationPackage package)
    {
        if (IsHost)
            return;

        CarOrientation[] cars = new CarOrientation[] { package.car1, package.car2, package.car3, package.car4 };
        foreach (CarOrientation car in cars)
        {
            if (car.id == CarOrientation.CAR_NOT_USED)
                continue;

            int index = -1;

            for(int i=0;i<activeCars.Count;i++)
                if (activeCars[i].ownerId==car.id)
                {
                    index = i;
                    break;
                }

            if(index==-1)//the car needs to be registered
            {
                AddCar(car.id);
                index = activeCars.Count - 1;
            }
            
            //set position
            NetworkCarComponents ncc = activeCars[index].car.GetComponent<NetworkCarComponents>();
            
            ncc.car.position = new Vector3(car.carPosX, car.carPosY, car.carPosZ);
            ncc.car.rotation = Quaternion.Euler(car.carRotX, car.carRotY, car.carRotZ);


            ncc.wheelFl.position = new Vector3(car.wheelFlPosX, car.wheelFlPosY, car.wheelFlPosZ);
            ncc.wheelFl.rotation = Quaternion.Euler(car.wheelFlRotX, car.wheelFlRotY, car.wheelFlRotZ);

            ncc.wheelFr.position = new Vector3(car.wheelFrPosX, car.wheelFrPosY, car.wheelFrPosZ);
            ncc.wheelFr.rotation = Quaternion.Euler(car.wheelFrRotX, car.wheelFrRotY, car.wheelFrRotZ);

            ncc.wheelRl.position = new Vector3(car.wheelRlPosX, car.wheelRlPosY, car.wheelRlPosZ);
            ncc.wheelRl.rotation = Quaternion.Euler(car.wheelRlRotX, car.wheelRlRotY, car.wheelRlRotZ);

            ncc.wheelRr.position = new Vector3(car.wheelRrPosX, car.wheelRrPosY, car.wheelRrPosZ);
            ncc.wheelRr.rotation = Quaternion.Euler(car.wheelRrRotX, car.wheelRrRotY, car.wheelRrRotZ);
        }
    }

    [ServerRpc]
    private void ClientInputServerRpc(ulong clientId, float accelerationInput, float brakeInput, float steeringInput)
    {
        foreach(InstantiatedCar car in activeCars)
        {
            if (clientId==car.ownerId)
            {
                NetworkCarController ncc2 = car.car.GetComponent<NetworkCarController>();
                ncc2.AccelerationInput = accelerationInput;
                ncc2.BrakeInput = brakeInput;
                ncc2.SteeringInput = steeringInput;
            }
        }
    }
}
