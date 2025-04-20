using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//it is a buffer for the CarOrientation setting
//so that the gamemanager won't set the car's position in an arbitrary time
public class RacerClientOrientationDelay : MonoBehaviour
{
    private object orientationLock=new object();
    private CarOrientation orientation;
    public CarOrientation Orientation
    {
        get
        {
            CarOrientation returnValue;
            lock (orientationLock)
                returnValue = orientation;
            return returnValue;
        }
        set
        {
            lock (orientationLock)
                orientation = value;
        }
    }

    private void FixedUpdate()
    {
        CarOrientation orientation = Orientation;

        this.gameObject.GetComponent<IngameCarComponents>().LerpOrientation(orientation);

        RacerPlayerMultiplayerClient rpmc = null;
        if(this.gameObject.TryGetComponent<RacerPlayerMultiplayerClient>(out rpmc))
            rpmc.Velocity=new Vector3(orientation.velocityX, orientation.velocityY, orientation.velocityZ);
    }
}
