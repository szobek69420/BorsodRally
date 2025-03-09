using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class RacetrackGenerator : MonoBehaviour
{
    public int trackLength = 100;        // Number of track segments
    public float trackWidth = 5f;        // Width of the track
    public float perlinScaleXZ = 0.1f;  // Scale for the Perlin noise in XZ plane
    public float perlinScaleY = 0.1f;   // Scale for the Perlin noise in Y-axis
    public float perlinSpeedXZ = 0.1f;  // Speed of the Perlin noise for XZ directions over time
    public float perlinSpeedY = 0.1f;   // Speed of the Perlin noise for Y elevation over time
    public float smoothFactor = 0.05f;   // Amount of smoothing between track points
    public int seed = 42;               // Seed for the Perlin Noise generation
    public Material trackMaterial;      // Material to apply to the track surface

    private List<Vector3> trackPoints = new List<Vector3>();   // List to hold track points
    private List<Vector3> trackPath = new List<Vector3>();     // Final smooth path for the track
    private MeshFilter meshFilter;   // MeshFilter to apply the mesh to the object
    private MeshRenderer meshRenderer; // MeshRenderer to apply the material
    private MeshCollider trackCollider;

    void Start()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();  // Add a MeshFilter to the object
        meshRenderer = gameObject.AddComponent<MeshRenderer>();  // Add a MeshRenderer to the object

        // Apply the provided material to the track surface
        if (trackMaterial != null)
        {
            meshRenderer.material = trackMaterial;
        }

        // Set the random seed for consistency
        UnityEngine.Random.InitState(seed);

        GenerateTrack();
    }

    void Update()
    {
        // Optionally update the track if you want dynamic generation
    }

    void GenerateTrack()
    {
        trackPoints.Clear();

        // Start the track at (0, 0, 0)
        float currentX = 0f;
        float currentY = 0f;
        float currentZ = 0f;

        trackPoints.Add(new Vector3(currentX, currentY, currentZ)); // Add the starting point (0, 0, 0)

        for (int i = 1; i < trackLength; i++) // Start from 1 since the first point is already added
        {
            // Generate the XZ direction using Perlin noise
            float perlinX = Mathf.PerlinNoise(i * perlinScaleXZ, currentX * perlinSpeedXZ + seed); // Seeded noise
            float perlinZ = Mathf.PerlinNoise(i * perlinScaleXZ, currentZ * perlinSpeedXZ + seed); // Seeded noise for Z direction

            // Use the Perlin noise to determine the direction of movement
            float moveX = Mathf.Cos(perlinX * Mathf.PI * 2f) * trackWidth; // smooth direction on X
            float moveZ = Mathf.Sin(perlinZ * Mathf.PI * 2f) * trackWidth; // smooth direction on Z

            // Move the current position in the XZ plane based on Perlin noise
            currentX += moveX;
            currentZ += moveZ;

            // Calculate the Y value using Perlin noise for elevation and smoothness
            currentY = Mathf.PerlinNoise(currentX * perlinScaleY, currentY * perlinSpeedY + seed) * 5f; // Y elevation using Perlin noise
            currentY += Mathf.Sin(currentX * 0.1f) * 2f; // Add some sinusoidal variation to the height
            //currentY = 0;

            // Add the new point to the track
            trackPoints.Add(new Vector3(currentX, currentY, currentZ));
        }

        // Smooth the Y-axis elevation using Catmull-Rom spline
        trackPath = CatmullRomSpline(trackPoints, smoothFactor);

        // Create the mesh for the track surface
        meshFilter.mesh = CreateRacetrackMesh(trackPath);
        gameObject.AddComponent<MeshCollider>();
    }

    Mesh CreateRacetrackMesh(List<Vector3> points) 
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        int vertexIndex = 0;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Count - 1)
            {
                forward += points[i + 1] - points[i];
            }
            if (i > 0)
            {
                forward += points[i] - points[i-1];
            }
            forward.Normalize();

            Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

            vertices.Add(points[i] + left * trackWidth * 0.5f);
            vertices.Add(points[i] - left * trackWidth * 0.5f);

            if (i < points.Count - 1)
            {
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);

                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
            }
            vertexIndex += 2;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // Catmull-Rom Spline function to smooth the path
    List<Vector3> CatmullRomSpline(List<Vector3> points, float smoothFactor)
    {
        List<Vector3> smoothPath = new List<Vector3>();

        for (int i = 0; i < points.Count - 2; i++)
        {
            Vector3 p0 = points[i];

            if (i == 0)
            {
                p0.y = 0;
                p0.x = 0;
                p0.z = 0;
            }
            else
            {
                p0 = points[i - 1];
            }
            Vector3 p1 = points[i];
            Vector3 p2 = points[i + 1];
            Vector3 p3 = points[i + 2];

            // Generate points between p1 and p2 using the Catmull-Rom interpolation formula
            for (float t = 0f; t <= 1f; t += smoothFactor)
            {
                Vector3 smoothPoint = CatmullRom(p0, p1, p2, p3, t);
                smoothPath.Add(smoothPoint);
            }
        }

        return smoothPath;
    }

    // Catmull-Rom interpolation formula
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
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