using System;
using System.Collections.Generic;
using UnityEngine;

public class RacetrackGenerator : MonoBehaviour
{
    public int seed = 42;                   // Random seed for control point generation.
    public int numberOfControlPoints = 100;  // Number of control points to generate.
    public float trackLength = 100f;        // Length of the track.
    public float trackWidth = 10f;           // Width of the track.
    public float elevation = 5f;
    public int gridSize = 10;
    public GameObject surfaceObject;

    private List<Vector3> controlPoints = new List<Vector3>();  // List of control points for the track.
    private List<Vector3> trackPoints = new List<Vector3>();    // Points along the generated track.

    void Start()
    {
        GenerateControlPoints();  // Generate control points based on the seed.
        GenerateTrackPoints();          // Generate track from the control points.

        Mesh mesh = CreateSurfaceMesh(trackPoints);

        // Apply the mesh to the GameObject
        if (surfaceObject != null)
        {
            surfaceObject.GetComponent<MeshFilter>().mesh = mesh;
        }
    }

    // Function to generate random control points based on a seed.
    void GenerateControlPoints()
    {
        controlPoints.Clear();
        UnityEngine.Random.InitState(seed);  // Initialize random number generator with the seed.

        Vector3 startPoint = new Vector3(0, 0, 0);  // Starting point at (0, 0, 0).
        controlPoints.Add(startPoint);

        // Generate control points along the track.
        for (int i = 1; i < numberOfControlPoints; i++)
        {
            float x = i * (trackLength / (numberOfControlPoints - 1));  // Distribute points along the x-axis.
            float z = UnityEngine.Random.Range(-trackWidth / 2, trackWidth / 2);   // Randomize the track's deviation in the z-axis.
            float y = UnityEngine.Random.Range(-elevation, elevation);  // Optional: random y for elevation changes.

            Vector3 controlPoint = new Vector3(x, y, z);
            controlPoints.Add(controlPoint);
        }

        // Add an endpoint at the end of the track to complete the path.
        Vector3 endPoint = new Vector3(trackLength, 0, 0);
        controlPoints.Add(endPoint);
    }

    // Function to generate the track using Catmull-Rom spline.
    void GenerateTrackPoints()
    {
        trackPoints.Clear();

        // Iterate through each set of four control points to generate segments.
        for (int i = 0; i < controlPoints.Count - 2; i++)
        {
            Vector3 p0 = controlPoints[i];

            if (i == 0)
            {
                p0.y = 0;
                p0.x = 0;
                p0.z = 0;
            }
            else
            {
                p0 = controlPoints[i - 1];
            }

            Vector3 p1 = controlPoints[i];
            Vector3 p2 = controlPoints[i + 1];
            Vector3 p3 = controlPoints[i + 2];

            // Generate points along the Catmull-Rom spline
            for (int j = 0; j <= 5; j++)  // 10 segments per curve
            {
                float t = j / 5f;  // Interpolation value between 0 and 1
                Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                trackPoints.Add(point);
            }
        }
    }

    // Catmull-Rom spline function to interpolate between four points.
    Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Vector3 result = 0.5f * (
            (2.0f * p1) +
            (-p0 + p2) * t +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
            (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
        );

        return result;
    }

    Mesh CreateSurfaceMesh(List<Vector3> splinePoints)
    {
        int splineCount = splinePoints.Count;

        // Step 1: Create the vertices grid
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        for (int i = 0; i < splineCount; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                // Sweep the spline points along the Y-axis (for example)
                Vector3 vertex = splinePoints[i] + new Vector3(0, 0, -j * 1.0f);  // Change this as needed
                vertices.Add(vertex);

                // UV mapping based on the position of the vertex in the grid
                uv.Add(new Vector2((float)i / (splineCount - 1), (float)j / (gridSize - 1)));
            }
        }

        // Step 2: Create the triangles for the mesh
        List<int> triangles = new List<int>();
        for (int i = 0; i < splineCount - 1; i++)
        {
            for (int j = 0; j < gridSize - 1; j++)
            {
                int current = i * gridSize + j;
                int next = current + 1;
                int above = (i + 1) * gridSize + j;
                int aboveNext = above + 1;

                // Create two triangles for each quad
                triangles.Add(current);
                triangles.Add(above);
                triangles.Add(next);

                triangles.Add(next);
                triangles.Add(above);
                triangles.Add(aboveNext);
            }
        }

        // Step 3: Create the mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uv.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}
