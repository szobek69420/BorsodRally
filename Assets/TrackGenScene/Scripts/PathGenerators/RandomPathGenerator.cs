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
        float diffBetweenDir = Mathf.Lerp(15f, 60f, curviness / 20);            //the maximal degrees of diff between two directions consecutively

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
            prevDir.y = 0f;

            Vector3 nextDir = Quaternion.Euler(0, currentRotation, 0) * prevDir;
            nextDir.y = currentElevation;

            trackPoints.Add((trackPoints[i - 1] + nextDir));
        }
        return trackPoints;
    }
}
