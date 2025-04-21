using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarComponents : MonoBehaviour
{
    private const float EPSILON = 0.1f;
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
        const float CORRECTION_LERP_STRENGTH = 0.1f;
       
        #region LongCalc
        Vector3 estimatedCarPos = new Vector3(currentOrientation.carPosX, currentOrientation.carPosY, currentOrientation.carPosZ)+
            (Time.time-currentOrientationTime)*new Vector3(currentOrientation.velocityX, currentOrientation.velocityY, currentOrientation.velocityZ);
        Quaternion estimatedCarRot = Quaternion.Euler(currentOrientation.carRotX, currentOrientation.carRotY, currentOrientation.carRotZ);


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
        if (Quaternion.Dot(car.transform.rotation, estimatedCarRot) > 1-EPSILON)
            car.transform.rotation = Quaternion.Lerp(car.transform.rotation, estimatedCarRot, CORRECTION_LERP_STRENGTH);
        else car.transform.rotation = estimatedCarRot;

        if ((wheelFl.transform.localPosition - estimatedWheelFlPos).sqrMagnitude > SQR_EPSILON)
            wheelFl.transform.localPosition = Vector3.Lerp(wheelFl.transform.localPosition, estimatedWheelFlPos, CORRECTION_LERP_STRENGTH);
        else wheelFl.transform.localPosition = estimatedWheelFlPos;
        if ((wheelFr.transform.localPosition - estimatedWheelFrPos).sqrMagnitude > SQR_EPSILON)
            wheelFr.transform.localPosition = Vector3.Lerp(wheelFr.transform.localPosition, estimatedWheelFrPos, CORRECTION_LERP_STRENGTH);
        else wheelFr.transform.localPosition = estimatedWheelFrPos;
        if ((wheelRl.transform.localPosition - estimatedWheelRlPos).sqrMagnitude > SQR_EPSILON)
            wheelRl.transform.localPosition = Vector3.Lerp(wheelRl.transform.localPosition, estimatedWheelRlPos, CORRECTION_LERP_STRENGTH);
        else wheelRl.transform.localPosition = estimatedWheelRlPos;
        if ((wheelRr.transform.localPosition - estimatedWheelRrPos).sqrMagnitude > SQR_EPSILON)
            wheelRr.transform.localPosition = Vector3.Lerp(wheelRr.transform.localPosition, estimatedWheelRrPos, CORRECTION_LERP_STRENGTH);
        else wheelRr.transform.localPosition = estimatedWheelRrPos;

        if (Quaternion.Dot(wheelFl.transform.localRotation, estimatedWheelFlRot) > 1 - EPSILON)
            wheelFl.transform.localRotation = Quaternion.Lerp(wheelFl.transform.localRotation, estimatedWheelFlRot, CORRECTION_LERP_STRENGTH);
        else wheelFl.transform.localRotation = estimatedWheelFlRot;
        if (Quaternion.Dot(wheelFr.transform.localRotation, estimatedWheelFrRot) > 1 - EPSILON)
            wheelFr.transform.localRotation = Quaternion.Lerp(wheelFr.transform.localRotation, estimatedWheelFrRot, CORRECTION_LERP_STRENGTH);
        else wheelFr.transform.localRotation = estimatedWheelFrRot;
        if (Quaternion.Dot(wheelRl.transform.localRotation, estimatedWheelRlRot) > 1 - EPSILON)
            wheelRl.transform.localRotation = Quaternion.Lerp(wheelRl.transform.localRotation, estimatedWheelRlRot, CORRECTION_LERP_STRENGTH);
        else wheelRl.transform.localRotation = estimatedWheelRlRot;
        if (Quaternion.Dot(wheelRr.transform.localRotation, estimatedWheelRrRot) > 1 - EPSILON)
            wheelRr.transform.localRotation = Quaternion.Lerp(wheelRr.transform.localRotation, estimatedWheelRrRot, CORRECTION_LERP_STRENGTH);
        else wheelRr.transform.localRotation = estimatedWheelRrRot;
    }

    public void SetCurrentOrientation(CarOrientation co)
    {
        currentOrientation = co;
        currentOrientationTime = Time.time;
    }
}
