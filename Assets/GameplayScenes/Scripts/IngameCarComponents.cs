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


    private CarOrientation currentOrientation = new CarOrientation(69);
    private float currentOrientationTime = 0.0f;//the Time.time value at setting the currentOrientation

    //sets the ownerId to 0
    public CarOrientation GetOrientation()
    {
        return new CarOrientation(0, car, wheelFl, wheelFr, wheelRl, wheelRr, rb.velocity, rb.angularVelocity);
    }

    public void SetOrientation(CarOrientation co)
    {
        SetCurrentOrientation(co);

        car.transform.position = new Vector3(currentOrientation.carPosX, currentOrientation.carPosY, currentOrientation.carPosZ);
        car.transform.rotation = Quaternion.Euler(currentOrientation.carRotX, currentOrientation.carRotY, currentOrientation.carRotZ);

        wheelFl.transform.localPosition = new Vector3(currentOrientation.wheelFlPosX, currentOrientation.wheelFlPosY, currentOrientation.wheelFlPosZ);
        wheelFr.transform.localPosition = new Vector3(currentOrientation.wheelFrPosX, currentOrientation.wheelFrPosY, currentOrientation.wheelFrPosZ);
        wheelRl.transform.localPosition = new Vector3(currentOrientation.wheelRlPosX, currentOrientation.wheelRlPosY, currentOrientation.wheelRlPosZ);
        wheelRr.transform.localPosition = new Vector3(currentOrientation.wheelRrPosX, currentOrientation.wheelRrPosY, currentOrientation.wheelRrPosZ);

        wheelFl.transform.localRotation = Quaternion.Euler(currentOrientation.wheelFlRotX, currentOrientation.wheelFlRotY, currentOrientation.wheelFlRotZ);
        wheelFr.transform.localRotation = Quaternion.Euler(currentOrientation.wheelFrRotX, currentOrientation.wheelFrRotY, currentOrientation.wheelFrRotZ);
        wheelRl.transform.localRotation = Quaternion.Euler(currentOrientation.wheelRlRotX, currentOrientation.wheelRlRotY, currentOrientation.wheelRlRotZ);
        wheelRr.transform.localRotation = Quaternion.Euler(currentOrientation.wheelRrRotX, currentOrientation.wheelRrRotY, currentOrientation.wheelRrRotZ);

        //if (rb != null)
        //    rb.velocity = new Vector3(currentOrientation.velocityX, currentOrientation.velocityY, currentOrientation.velocityZ);
    }

    public void ExtrapolateOrientation()
    {
        const float CORRECTION_LERP_STRENGTH = 0.2f;
       
        #region LongCalc
        Vector3 estimatedCarPos = new Vector3(currentOrientation.carPosX, currentOrientation.carPosY, currentOrientation.carPosZ)+
            (Time.time-currentOrientationTime)*new Vector3(currentOrientation.velocityX, currentOrientation.velocityY, currentOrientation.velocityZ);
        
        Vector3 helper=new Vector3(currentOrientation.angularVelocityX, currentOrientation.angularVelocityY, currentOrientation.angularVelocityZ);
        Quaternion estimatedCarRot = 
            Quaternion.AngleAxis(
                (Time.time - currentOrientationTime)*Mathf.Rad2Deg*helper.magnitude,
                helper.normalized
            ) *
            Quaternion.Euler(currentOrientation.carRotX, currentOrientation.carRotY, currentOrientation.carRotZ);


        Vector3 estimatedWheelFlPos= new Vector3(currentOrientation.wheelFlPosX, currentOrientation.wheelFlPosY, currentOrientation.wheelFlPosZ);
        Quaternion estimatedWheelFlRot = Quaternion.Euler(currentOrientation.wheelFlRotX, currentOrientation.wheelFlRotY, currentOrientation.wheelFlRotZ);

        Vector3 estimatedWheelFrPos = new Vector3(currentOrientation.wheelFrPosX, currentOrientation.wheelFrPosY, currentOrientation.wheelFrPosZ);
        Quaternion estimatedWheelFrRot = Quaternion.Euler(currentOrientation.wheelFrRotX, currentOrientation.wheelFrRotY, currentOrientation.wheelFrRotZ);

        Vector3 estimatedWheelRlPos = new Vector3(currentOrientation.wheelRlPosX, currentOrientation.wheelRlPosY, currentOrientation.wheelRlPosZ);
        Quaternion estimatedWheelRlRot = Quaternion.Euler(currentOrientation.wheelRlRotX, currentOrientation.wheelRlRotY, currentOrientation.wheelRlRotZ);

        Vector3 estimatedWheelRrPos = new Vector3(currentOrientation.wheelRrPosX, currentOrientation.wheelRrPosY, currentOrientation.wheelRrPosZ);
        Quaternion estimatedWheelRrRot = Quaternion.Euler(currentOrientation.wheelRrRotX, currentOrientation.wheelRrRotY, currentOrientation.wheelRrRotZ);
        #endregion

        if ((car.transform.position - estimatedCarPos).sqrMagnitude > SQR_EPSILON)
            car.transform.position = Vector3.Lerp(car.transform.position, estimatedCarPos, CORRECTION_LERP_STRENGTH);
        else car.transform.position = estimatedCarPos;
        if (Quaternion.Dot(car.transform.rotation, estimatedCarRot) < 1-EPSILON)
            car.transform.rotation = Quaternion.Lerp(car.transform.rotation, estimatedCarRot, CORRECTION_LERP_STRENGTH);
        else car.transform.rotation = estimatedCarRot;

        wheelFl.transform.localPosition = estimatedWheelFlPos;
        wheelFr.transform.localPosition = estimatedWheelFrPos;
        wheelRl.transform.localPosition = estimatedWheelRlPos;
        wheelRr.transform.localPosition = estimatedWheelRrPos;

        wheelFl.transform.localRotation = estimatedWheelFlRot;
        wheelFr.transform.localRotation = estimatedWheelFrRot;
        wheelRl.transform.localRotation = estimatedWheelRlRot;
        wheelRr.transform.localRotation = estimatedWheelRrRot;
    }

    public void SetCurrentOrientation(CarOrientation co)
    {
        currentOrientation = co;
        currentOrientationTime = Time.time;
    }
}
