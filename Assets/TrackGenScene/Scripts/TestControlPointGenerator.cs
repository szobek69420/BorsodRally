using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestControlPointGenerator : ControlPointGeneratorBase
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

        float currentX = 0f;
        float currentY = 0f;
        float currentZ = 0f;

        trackPoints.Add(new Vector3(currentX, currentY, currentZ)); // Add the starting point (0, 0, 0)

        for (int i = 1; i < length; i++)
        {
            // Generate the directions using Perlin noise 
            //currentX = Mathf.PerlinNoise(i * seed + currentX, seed + currentX);
            currentZ = Mathf.PerlinNoise(i * seed + currentZ, seed + currentZ);
            currentY = Mathf.PerlinNoise(seed * currentY, 0.01f * currentY + seed) * 5f;

            currentX += Mathf.Cos(currentX * Mathf.PI * 2f) * 15 * 3;
            currentZ += Mathf.Sin(currentZ * Mathf.PI * 2f) * 15 * curviness;
            currentY += Mathf.Sin(currentX * 0.1f) * 10f * elevation;

            trackPoints.Add(new Vector3(currentX, currentY, currentZ));
        }
        return trackPoints;
    }

}
