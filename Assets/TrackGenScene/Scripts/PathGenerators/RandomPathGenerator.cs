using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomPathGenerator : PathGeneratorBase
{
    protected override List<Vector3> GenerateControlPoints(int seed, int length, float curviness, float elevation)
    {
        List<Vector3> trackPoints = new List<Vector3>();
        Random.InitState(seed);

        int pointDistance = 20;                                         //the fixed distance between points
        float diffBetweenDir = 5f;                                      //the quantity of difference between two directions
        float maxTrackHeight = 25f;
        float currentRotation = 0f;
        float currentElevation = 0f;

        bool legitTrack = false;

        while (!legitTrack)
        {
            trackPoints.Clear();
            trackPoints.Add(new Vector3(0f, 0f, 0f));
            trackPoints.Add(new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)));

            trackPoints[1] = trackPoints[1].normalized * pointDistance;

            for (int i = 2; i < length; i++)
            {
                // Generate the directions 
                currentRotation = Random.Range(-diffBetweenDir, diffBetweenDir);
                currentElevation = Random.Range(-elevation, elevation);

                Vector3 prevDir = trackPoints[i - 1] - trackPoints[i - 2];

                Vector3 nextDir = Quaternion.Euler(0, currentRotation * curviness, 0) * prevDir;

                if (((i - 2) % 5 == 0) && i > 2)
                {
                    if (trackPoints[i - 1].y + currentElevation > maxTrackHeight ||
                            trackPoints[i - 1].y + currentElevation < -maxTrackHeight)
                    {
                        currentElevation *= -1;
                    }

                    for (int j = 1; j < 6; j++)
                    {
                        Vector3 point = trackPoints[i - j];
                        point.y = Mathf.Lerp(trackPoints[i - 5].y + currentElevation, trackPoints[i - 5].y, 0.2f * j);
                        trackPoints[i - j] = point;
                    }
                }
                nextDir = nextDir.normalized * pointDistance;
                trackPoints.Add((trackPoints[i - 1] + nextDir));
            }
            //check for valid track
            legitTrack = CheckTrackValidity(trackPoints, pointDistance);
        }
        return trackPoints;
    }

    private bool CheckTrackValidity(List<Vector3> trackPoints, int minDistance)
    {
        Vector2 v1 = Vector2.zero;
        Vector2 v2 = Vector2.zero;
        Vector2 v3 = Vector2.zero;
        Vector2 v4 = Vector2.zero;
        Vector2 v5 = Vector2.zero;

        for (int i = 1; i < trackPoints.Count - 2; i++)
        {
            for (int j = i + 2; j < trackPoints.Count; j++)
            {
                v1 = new Vector2(trackPoints[i - 1].x, trackPoints[i-1].z);
                v2 = new Vector2(trackPoints[i].x, trackPoints[i].z);
                v3 = new Vector2(trackPoints[j - 1].x, trackPoints[j - 1].z);
                v4 = new Vector2(trackPoints[j].x, trackPoints[j].z);

                if (IsIntersects(v1,v2,v3,v4)) return false;
            }
        }
        return true;
    }

    private bool IsIntersects(Vector2 v1, Vector2 v2, Vector2 v3, Vector2 v4)
    {
        float o1 = (v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x);
        float o2 = (v2.x - v1.x) * (v4.y - v1.y) - (v2.y - v1.y) * (v4.x - v1.x);
        float o3 = (v4.x - v3.x) * (v1.y - v3.y) - (v4.y - v3.y) * (v1.x - v3.x);
        float o4 = (v4.x - v3.x) * (v2.y - v3.y) - (v4.y - v3.y) * (v2.x - v3.x);

        float tolerance = 0.0001f;

        if (o1 * o2 < -tolerance && o3 * o4 < -tolerance)
        {
            return true;
        }
        return false;
    }
}



/*for (int j = 1; j < trackPoints.Count - 4; j++)
       {
           Vector3 point1 = trackPoints[i];
           Vector3 point2 = trackPoints[j];

           float y = trackPoints[i].y;

           point1.y = 0;
           point2.y = 0;

           int counter = 0;

           bool trDeg = rotFromStart > 0;
           bool dirDeg = Vector3.SignedAngle(trackPoints[i] - trackPoints[i - 1], trackPoints[j] - trackPoints[i - 1], Vector3.down) > 0;

           if (trDeg && dirDeg) currentRotation = -1 * Mathf.Abs(currentRotation);
           else if (trDeg && !dirDeg) currentRotation = -1 * Mathf.Abs(currentRotation);
           else if (!trDeg && dirDeg) currentRotation = Mathf.Abs(currentRotation);
           else if (!trDeg && !dirDeg) currentRotation = Mathf.Abs(currentRotation);

           while (Vector3.Distance(point1, point2) < (pointDistance * 2f))
           {
               //Debug.Log(Vector3.SignedAngle(trackPoints[i] - trackPoints[i - 1], trackPoints[j] - trackPoints[i - 1], Vector3.down));
               //Debug.Log(currentRotation);
               //Debug.Log(trackPoints[i] - trackPoints[i - 1]);
               //Debug.Log(trackPoints[j] - trackPoints[i - 1]);
               Debug.DrawLine(trackPoints[i], trackPoints[i - 1], Color.red, 10000);
               Debug.DrawLine(trackPoints[j], trackPoints[i - 1], Color.red, 10000);      

               nextDir = Quaternion.Euler(0f, -currentRotation, 0f) * nextDir;
               nextDir = nextDir.normalized * pointDistance;

               Vector3 newPoint = trackPoints[i - 1] + nextDir;
               newPoint.y = y;
               trackPoints[i] = newPoint;
               point1 = trackPoints[i];
               point1.y = 0;

               if (counter > 200) break;
               counter++;
           }
       }
       rotFromStart += Vector3.SignedAngle(prevDir, nextDir, Vector3.down);
       Debug.Log(rotFromStart  );
       //if (rotFromStart > 360) rotFromStart -= 360;
       //if (rotFromStart < -360) rotFromStart += 360;*/