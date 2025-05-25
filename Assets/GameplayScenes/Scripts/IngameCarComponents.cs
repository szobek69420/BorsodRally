using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarComponents : MonoBehaviour
{
    private const float EPSILON = 0.5f;
    private const float SQR_EPSILON = EPSILON*EPSILON;

    public Transform car;

    public Transform wheelFl;
    public Transform wheelFr;
    public Transform wheelRl;
    public Transform wheelRr;

    public Rigidbody rb;

    private List<CarOrientation> interpolationBuffer = new List<CarOrientation>();

    //sets the ownerId to 0
    public CarOrientation GetOrientation()
    {
        return new CarOrientation(0, "", car, wheelFl, wheelFr, wheelRl, wheelRr, rb.velocity, rb.angularVelocity, 0.0f);
    }

    public void SetOrientation(CarOrientation co)
    {
        car.transform.position = new Vector3(co.carPosX, co.carPosY, co.carPosZ);
        car.transform.rotation = Quaternion.Euler(co.carRotX, co.carRotY, co.carRotZ);

        wheelFl.transform.localPosition = new Vector3(co.wheelFlPosX, co.wheelFlPosY, co.wheelFlPosZ);
        wheelFr.transform.localPosition = new Vector3(co.wheelFrPosX, co.wheelFrPosY, co.wheelFrPosZ);
        wheelRl.transform.localPosition = new Vector3(co.wheelRlPosX, co.wheelRlPosY, co.wheelRlPosZ);
        wheelRr.transform.localPosition = new Vector3(co.wheelRrPosX, co.wheelRrPosY, co.wheelRrPosZ);

        wheelFl.transform.localRotation = Quaternion.Euler(co.wheelFlRotX, co.wheelFlRotY, co.wheelFlRotZ);
        wheelFr.transform.localRotation = Quaternion.Euler(co.wheelFrRotX, co.wheelFrRotY, co.wheelFrRotZ);
        wheelRl.transform.localRotation = Quaternion.Euler(co.wheelRlRotX, co.wheelRlRotY, co.wheelRlRotZ);
        wheelRr.transform.localRotation = Quaternion.Euler(co.wheelRrRotX, co.wheelRrRotY, co.wheelRrRotZ);

        //if (rb != null)
        //    rb.velocity = new Vector3(currentOrientation.velocityX, currentOrientation.velocityY, currentOrientation.velocityZ);
    }

    //interpolates between two orientations in the interpolation buffer
    //the (serverTime-delay) must be greater than in the last call
    public void InterpolateOrientation(float serverTime, float delay)
    {
        float time = serverTime - delay;

        CarOrientation o1, o2;
        o1= new CarOrientation();//the constructor calls are here to make intellisense shut up
        o2= new CarOrientation();

        //get the car orientations
        if (interpolationBuffer.Count < 2)
            return;
        for (int j = 0; j < interpolationBuffer.Count; j++)
        {
            if (j == interpolationBuffer.Count - 2||
                (interpolationBuffer[j].networkTime<=time&& interpolationBuffer[j + 1].networkTime > time))
            {
                o1= interpolationBuffer[j];
                o2= interpolationBuffer[j + 1];
                break;
            }
        }

        //remove unnecessary orientations
        for(int j=0;j< interpolationBuffer.Count-2;j++)
        {
            if(time<interpolationBuffer[j].networkTime)//get the first orientation that is not yet reached and remove every orientation before that, except for the last one
            {
                for (int k = j - 2; k >= 0; k--)
                    interpolationBuffer.RemoveAt(k);
                break;
            }
        }

        float i = (time-o1.networkTime)/(o2.networkTime - o1.networkTime); //interpolation coefficient

        //interpolate
        Vector3 carPos, wheelFlPos, wheelFrPos, wheelRlPos, wheelRrPos;
        Quaternion carRot, wheelFlRot, wheelFrRot, wheelRlRot, wheelRrRot;

        carPos = Vector3.Lerp(new Vector3(o1.carPosX, o1.carPosY, o1.carPosZ), new Vector3(o2.carPosX, o2.carPosY, o2.carPosZ), i);
        wheelFlPos = Vector3.Lerp(new Vector3(o1.wheelFlPosX, o1.wheelFlPosY, o1.wheelFlPosZ), new Vector3(o2.wheelFlPosX, o2.wheelFlPosY, o2.wheelFlPosZ), i);
        wheelFrPos = Vector3.Lerp(new Vector3(o1.wheelFrPosX, o1.wheelFrPosY, o1.wheelFrPosZ), new Vector3(o2.wheelFrPosX, o2.wheelFrPosY, o2.wheelFrPosZ), i);
        wheelRlPos = Vector3.Lerp(new Vector3(o1.wheelRlPosX, o1.wheelRlPosY, o1.wheelRlPosZ), new Vector3(o2.wheelRlPosX, o2.wheelRlPosY, o2.wheelRlPosZ), i);
        wheelRrPos = Vector3.Lerp(new Vector3(o1.wheelRrPosX, o1.wheelRrPosY, o1.wheelRrPosZ), new Vector3(o2.wheelRrPosX, o2.wheelRrPosY, o2.wheelRrPosZ), i);

        carRot = Quaternion.Lerp(Quaternion.Euler(o1.carRotX, o1.carRotY, o1.carRotZ), Quaternion.Euler(o2.carRotX, o2.carRotY, o2.carRotZ), i);
        wheelFlRot = Quaternion.Lerp(Quaternion.Euler(o1.wheelFlRotX, o1.wheelFlRotY, o1.wheelFlRotZ), Quaternion.Euler(o2.wheelFlRotX, o2.wheelFlRotY, o2.wheelFlRotZ), i);
        wheelFrRot = Quaternion.Lerp(Quaternion.Euler(o1.wheelFrRotX, o1.wheelFrRotY, o1.wheelFrRotZ), Quaternion.Euler(o2.wheelFrRotX, o2.wheelFrRotY, o2.wheelFrRotZ), i);
        wheelRlRot = Quaternion.Lerp(Quaternion.Euler(o1.wheelRlRotX, o1.wheelRlRotY, o1.wheelRlRotZ), Quaternion.Euler(o2.wheelRlRotX, o2.wheelRlRotY, o2.wheelRlRotZ), i);
        wheelRrRot = Quaternion.Lerp(Quaternion.Euler(o1.wheelRrRotX, o1.wheelRrRotY, o1.wheelRrRotZ), Quaternion.Euler(o2.wheelRrRotX, o2.wheelRrRotY, o2.wheelRrRotZ), i);

        car.position = carPos; car.rotation = carRot;
        wheelFl.localPosition = wheelFlPos; wheelFl.localRotation = wheelFlRot;
        wheelFr.localPosition = wheelFrPos; wheelFr.localRotation = wheelFrRot;
        wheelRl.localPosition = wheelRlPos; wheelRl.localRotation = wheelRlRot;
        wheelRr.localPosition = wheelRrPos; wheelRr.localRotation = wheelRrRot;
    }

    public Vector3 InterpolateVelocity(float serverTime, float delay)
    {
        float time = serverTime - delay;


        //get the car orientations
        if (interpolationBuffer.Count < 2)
            return Vector3.zero;
        for (int j = 0; j < interpolationBuffer.Count; j++)
        {
            if (j == interpolationBuffer.Count - 2 ||
                (interpolationBuffer[j].networkTime <= time && interpolationBuffer[j + 1].networkTime > time))
            {
                float i= (time- interpolationBuffer[j].networkTime)/(interpolationBuffer[j+1].networkTime-interpolationBuffer[j].networkTime);

                return Vector3.Lerp(
                    new Vector3(
                        interpolationBuffer[j].velocityX,
                        interpolationBuffer[j].velocityY,
                        interpolationBuffer[j].velocityZ
                        ),
                    new Vector3(
                        interpolationBuffer[j+1].velocityX,
                        interpolationBuffer[j+1].velocityY,
                        interpolationBuffer[j+1].velocityZ
                        ),
                    i
                    );
            }
        }

        return Vector3.zero;
    }

    //adds the car orientation to the interpolation buffer
    public void AddOrientationToBuffer(CarOrientation co)
    {
        interpolationBuffer.Add(co);
    }
}
