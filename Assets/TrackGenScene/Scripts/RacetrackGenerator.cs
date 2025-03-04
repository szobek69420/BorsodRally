/*using System;
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
}*/

using UnityEngine;
using System.Collections.Generic;

public class RacetrackGenerator : MonoBehaviour
{
    public int trackLength = 100;        // Number of track segments
    public float trackWidth = 5f;        // Width of the track
    public float perlinScaleXZ = 0.1f;  // Scale for the Perlin noise in XZ plane
    public float perlinScaleY = 0.1f;   // Scale for the Perlin noise in Y-axis
    public float perlinSpeedXZ = 0.1f;  // Speed of the Perlin noise for XZ directions over time
    public float perlinSpeedY = 0.1f;   // Speed of the Perlin noise for Y elevation over time
    public float smoothFactor = 0.05f;   // Amount of smoothing between track points
    public Material trackMaterial;      // Material to apply to the track surface
    public int seed = 42;               // Seed for the Perlin Noise generation

    private List<Vector3> trackPoints = new List<Vector3>();   // List to hold track points
    private List<Vector3> trackPath = new List<Vector3>();     // Final smooth path for the track
    private MeshFilter meshFilter;   // MeshFilter to apply the mesh to the object
    private MeshRenderer meshRenderer; // MeshRenderer to apply the material

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
        Random.InitState(seed);

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
            float perlinX = Mathf.PerlinNoise(i * perlinScaleXZ, Time.time * perlinSpeedXZ + seed); // Seeded noise
            float perlinZ = Mathf.PerlinNoise(i * perlinScaleXZ + 1000f, Time.time * perlinSpeedXZ + seed); // Seeded noise for Z direction

            // Use the Perlin noise to determine the direction of movement
            float moveX = Mathf.Cos(perlinX * Mathf.PI * 2f) * trackWidth; // smooth direction on X
            float moveZ = Mathf.Sin(perlinZ * Mathf.PI * 2f) * trackWidth; // smooth direction on Z

            // Move the current position in the XZ plane based on Perlin noise
            currentX += moveX;
            currentZ += moveZ;

            // Calculate the Y value using Perlin noise for elevation and smoothness
            currentY = Mathf.PerlinNoise(currentX * perlinScaleY, Time.time * perlinSpeedY + seed) * 5f; // Y elevation using Perlin noise
            currentY += Mathf.Sin(currentX * 0.1f) * 2f; // Add some sinusoidal variation to the height

            // Add the new point to the track
            trackPoints.Add(new Vector3(currentX, currentY, currentZ));
        }

        // Smooth the Y-axis elevation using Catmull-Rom spline
        trackPath = CatmullRomSpline(trackPoints, smoothFactor);

        // Create the mesh for the track surface
        CreateTrackMesh(trackPath);
    }

    // Create the mesh for the track surface
    /*void CreateTrackMesh(List<Vector3> path)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector3 p1 = path[i];
            Vector3 p2 = path[i + 1];

            // Create a perpendicular direction to generate the track width
            Vector3 direction = (p2 - p1).normalized;
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

            // Create vertices for the left and right side of the track
            Vector3 left = p1 - perpendicular * trackWidth * 0.5f;
            Vector3 right = p1 + perpendicular * trackWidth * 0.5f;
            Vector3 leftNext = p2 - perpendicular * trackWidth * 0.5f;
            Vector3 rightNext = p2 + perpendicular * trackWidth * 0.5f;

            // Add the vertices for the current segment
            vertices.Add(left);
            vertices.Add(right);
            vertices.Add(leftNext);
            vertices.Add(rightNext);

            // Add triangles (two triangles per segment to form a quad)
            int index = i * 4;

            // Fix the triangle connections to ensure no gaps
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2); // First triangle

            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 3); // Second triangle

            // Add UV coordinates (simple linear mapping)
            uv.Add(new Vector2((float)i / path.Count, 0f));
            uv.Add(new Vector2((float)i / path.Count, 1f));
            uv.Add(new Vector2((float)(i + 1) / path.Count, 0f));
            uv.Add(new Vector2((float)(i + 1) / path.Count, 1f));

            // Calculate normals (upward direction for the track surface)
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
        }

        // Create and set the mesh
        Mesh trackMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uv.ToArray(),
            normals = normals.ToArray()
        };

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = trackMesh;
    }*/


    void CreateTrackMesh(List<Vector3> smoothPath)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();

        for (int i = 0; i < smoothPath.Count - 1; i++)
        {
            Vector3 p1 = smoothPath[i];
            Vector3 p2 = smoothPath[i + 1];

            // Generate the direction vector of the path between p1 and p2
            Vector3 direction = (p2 - p1).normalized;

            // Create a perpendicular vector for the width of the track
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up).normalized;

            // Generate vertices on both sides of the track
            Vector3 left = p1 - perpendicular * trackWidth * 0.5f;
            Vector3 right = p1 + perpendicular * trackWidth * 0.5f;
            Vector3 leftNext = p2 - perpendicular * trackWidth * 0.5f;
            Vector3 rightNext = p2 + perpendicular * trackWidth * 0.5f;

            // Add vertices for the current segment
            vertices.Add(left);
            vertices.Add(right);
            vertices.Add(leftNext);
            vertices.Add(rightNext);

            // Add triangles (two triangles per segment to form a quad)
            int index = i * 4;
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2); // First triangle
            triangles.Add(index + 2);
            triangles.Add(index + 1);
            triangles.Add(index + 3); // Second triangle

            // Add UV coordinates (simple linear mapping)
            uv.Add(new Vector2((float)i / smoothPath.Count, 0f));
            uv.Add(new Vector2((float)i / smoothPath.Count, 1f));
            uv.Add(new Vector2((float)(i + 1) / smoothPath.Count, 0f));
            uv.Add(new Vector2((float)(i + 1) / smoothPath.Count, 1f));

            // Add normals (upward direction for the track surface)
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
            normals.Add(Vector3.up);
        }

        // Create and set the mesh
        Mesh trackMesh = new Mesh
        {
            vertices = vertices.ToArray(),
            triangles = triangles.ToArray(),
            uv = uv.ToArray(),
            normals = normals.ToArray()
        };

        // Assign the generated mesh to the MeshFilter
        meshFilter.mesh = trackMesh;
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