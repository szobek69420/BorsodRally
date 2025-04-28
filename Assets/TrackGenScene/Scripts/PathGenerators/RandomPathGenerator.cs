using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPathGenerator : PathGeneratorBase
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    protected override List<Vector3> GenerateControlPoints(int seed, int length, float curviness, float elevation)
    {
        List<Vector3> trackPoints = new List<Vector3>();
        Random.InitState(seed);
        int pointDistance = 20;                                                 //the fixed distance between points
        float diffBetweenDir = 5f;                                              //the quantity of difference between two directions

        float currentRotation = 0f;    
        float currentElevation = 0f;

        trackPoints.Add(new Vector3(0f, 0f, 0f));                               
        trackPoints.Add(new Vector3(Random.Range(-1f,1f), 0f, Random.Range(-1f, 1f)));

        trackPoints[1] *= pointDistance / trackPoints[1].magnitude;

        for (int i = 2; i < length; i++)
        {
            // Generate the directions 
            currentRotation = Random.Range(-diffBetweenDir, diffBetweenDir);
            currentElevation = Random.Range(-elevation, elevation);

            Vector3 prevDir = trackPoints[i-1] - trackPoints[i-2];

            Vector3 nextDir = Quaternion.Euler(0, currentRotation * curviness, 0) * prevDir;

            if (((i - 2) % 5 == 0) && i > 2)
            {
                currentElevation = Random.Range(-elevation, elevation);
                for(int j = 1; j < 6; j++)
                {
                    Vector3 point = trackPoints[i - j];
                    point.y = Mathf.Lerp(trackPoints[i - 5].y, trackPoints[i - 5].y + currentElevation, 0.2f * j);
                    trackPoints[i - j] = point;
                }
            }

            /*for(int j = 0; j < trackPoints.Count; j++)
            {
                if (Vector3.Distance(trackPoints[j], (trackPoints[i - 1] + nextDir)) < 15f)
                {
                    nextDir = Quaternion.Euler(0, (currentRotation * curviness) + 90f, 0) * prevDir;
                    //break;
                }
            }*/


            trackPoints.Add((trackPoints[i - 1] + nextDir));
        }
        return trackPoints;
    }
}
