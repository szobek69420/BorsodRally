using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameCarComponents : MonoBehaviour
{
    public Transform car;

    public Transform wheelFl;
    public Transform wheelFr;
    public Transform wheelRl;
    public Transform wheelRr;

    public Rigidbody rb;

    //sets the ownerId to 0
    public CarOrientation GetOrientation()
    {
        return new CarOrientation(0, car, wheelFl, wheelFr, wheelRl, wheelRr, rb.velocity);
    }

    public void SetOrientation(CarOrientation co)
    {
        car.transform.position=new Vector3(co.carPosX, co.carPosY, co.carPosZ);
        car.transform.rotation=Quaternion.Euler(co.carRotX, co.carRotY, co.carRotZ);

        wheelFl.transform.localPosition = new Vector3(co.wheelFlPosX, co.wheelFlPosY, co.wheelFlPosZ);
        wheelFr.transform.localPosition = new Vector3(co.wheelFrPosX, co.wheelFrPosY, co.wheelFrPosZ);
        wheelRl.transform.localPosition = new Vector3(co.wheelRlPosX, co.wheelRlPosY, co.wheelRlPosZ);
        wheelRr.transform.localPosition = new Vector3(co.wheelRrPosX, co.wheelRrPosY, co.wheelRrPosZ);

        wheelFl.transform.localRotation = Quaternion.Euler(co.wheelFlRotX, co.wheelFlRotY, co.wheelFlRotZ);
        wheelFr.transform.localRotation = Quaternion.Euler(co.wheelFrRotX, co.wheelFrRotY, co.wheelFrRotZ);
        wheelRl.transform.localRotation = Quaternion.Euler(co.wheelRlRotX, co.wheelRlRotY, co.wheelRlRotZ);
        wheelRr.transform.localRotation = Quaternion.Euler(co.wheelRrRotX, co.wheelRrRotY, co.wheelRrRotZ);

        if (rb!=null)
            rb.velocity=new Vector3(co.velocityX, co.velocityY, co.velocityZ);
    }

    public void LerpOrientation(CarOrientation co)
    {
        const float LERP_STRENGTH = 0.3f;

        car.transform.position = Vector3.Lerp(car.transform.position, new Vector3(co.carPosX, co.carPosY, co.carPosZ), LERP_STRENGTH);
        car.transform.rotation = Quaternion.Lerp(car.transform.rotation, Quaternion.Euler(co.carRotX, co.carRotY, co.carRotZ), LERP_STRENGTH);

        wheelFl.transform.localPosition = Vector3.Lerp(wheelFl.transform.localPosition, new Vector3(co.wheelFlPosX, co.wheelFlPosY, co.wheelFlPosZ), LERP_STRENGTH);
        wheelFr.transform.localPosition = Vector3.Lerp(wheelFr.transform.localPosition, new Vector3(co.wheelFrPosX, co.wheelFrPosY, co.wheelFrPosZ), LERP_STRENGTH);
        wheelRl.transform.localPosition = Vector3.Lerp(wheelRl.transform.localPosition, new Vector3(co.wheelRlPosX, co.wheelRlPosY, co.wheelRlPosZ), LERP_STRENGTH);
        wheelRr.transform.localPosition = Vector3.Lerp(wheelRr.transform.localPosition, new Vector3(co.wheelRrPosX, co.wheelRrPosY, co.wheelRrPosZ), LERP_STRENGTH);

        wheelFl.transform.localRotation = Quaternion.Lerp(wheelFl.transform.localRotation, Quaternion.Euler(co.wheelFlRotX, co.wheelFlRotY, co.wheelFlRotZ), LERP_STRENGTH);
        wheelFr.transform.localRotation = Quaternion.Lerp(wheelFr.transform.localRotation, Quaternion.Euler(co.wheelFrRotX, co.wheelFrRotY, co.wheelFrRotZ), LERP_STRENGTH);
        wheelRl.transform.localRotation = Quaternion.Lerp(wheelRl.transform.localRotation, Quaternion.Euler(co.wheelRlRotX, co.wheelRlRotY, co.wheelRlRotZ), LERP_STRENGTH);
        wheelRr.transform.localRotation = Quaternion.Lerp(wheelRr.transform.localRotation, Quaternion.Euler(co.wheelRrRotX, co.wheelRrRotY, co.wheelRrRotZ), LERP_STRENGTH);

        if (rb != null)
            rb.velocity = new Vector3(co.velocityX, co.velocityY, co.velocityZ);
    }
}
