using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PathGeneratorBase : MonoBehaviour
{
    public List<Vector3> GenerateTrackPoints(int seed, int length, float curviness, float elevation)
    {
        return CatmullRomSpline(GenerateControlPoints(seed, length, curviness, elevation));
    }

    protected virtual List<Vector3> GenerateControlPoints(int seed, int length, float curviness, float elevation) 
    { 
        return null; 
    }

    private List<Vector3> CatmullRomSpline(List<Vector3> trackPoints)
    {
        List<Vector3> smoothPath = new List<Vector3>();

        for (int i = 0; i < trackPoints.Count - 2; i++)
        {
            Vector3 p0 = trackPoints[i];

            if (i == 0)
            {
                p0.y = 0;
                p0.x = 0;
                p0.z = 0;
            }
            else
            {
                p0 = trackPoints[i - 1];
            }
            Vector3 p1 = trackPoints[i];
            Vector3 p2 = trackPoints[i + 1];
            Vector3 p3 = trackPoints[i + 2];

            for (float t = 0f; t <= 1f; t += 0.05f)
            {
                Vector3 smoothPoint = CatmullRom(p0, p1, p2, p3, t);
                smoothPath.Add(smoothPoint);
            }
        }

        return smoothPath;
    }

    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        float x = 0.5f * ((2f * p1.x) +
                          (-p0.x + p2.x) * t +
                          (2f * p0.x - 5f * p1.x + 4f * p2.x - p3.x) * t2 +
                          (-p0.x + 3f * p1.x - 3f * p2.x + p3.x) * t3);

        float y = 0.5f * ((2f * p1.y) +
                          (-p0.y + p2.y) * t +
                          (2f * p0.y - 5f * p1.y + 4f * p2.y - p3.y) * t2 +
                          (-p0.y + 3f * p1.y - 3f * p2.y + p3.y) * t3);

        float z = 0.5f * ((2f * p1.z) +
                          (-p0.z + p2.z) * t +
                          (2f * p0.z - 5f * p1.z + 4f * p2.z - p3.z) * t2 +
                          (-p0.z + 3f * p1.z - 3f * p2.z + p3.z) * t3);

        return new Vector3(x, y, z);
    }
}
