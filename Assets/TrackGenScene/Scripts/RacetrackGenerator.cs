using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;


public class RacetrackGenerator : MonoBehaviour
{
    [SerializeField] public int seed = 42;                                              // Seed for the Perlin Noise generation
    [SerializeField] public int trackLength = 100;                                      // Number of track segments
    [SerializeField] public int trackWidth = 5;                                         // Width of the track
    [SerializeField] public float perlinScaleZ = 1f;                                    // Scale for the Perlin noise in Z-axis
    [SerializeField] public float perlinScaleY = 0.05f;                                 // Scale for the Perlin noise in Y-axis 
    [SerializeField] public Material trackMaterial;                                     // Material to apply to the track surface

    [SerializeField] private List<Vector3> trackPoints = new List<Vector3>();    // List to hold track points
    [SerializeField] private MeshFilter meshFilter;                              // MeshFilter to apply the mesh to the object
    [SerializeField] private MeshRenderer meshRenderer;                          // MeshRenderer to apply the material

    void Start()
    {
        seed = PlayerPrefs.GetInt("seed");
        trackLength = PlayerPrefs.GetInt("length");
        perlinScaleZ = PlayerPrefs.GetFloat("curviness");

        StartGen();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) { ResetGen(); }
    }

    void StartGen()
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();

        UnityEngine.Random.InitState(seed);

        if (trackMaterial != null)
        {
            meshRenderer.material = trackMaterial;
        }

        GenerateSprintTrackPoints();
        //GenerateCircuitTrackPoints();

        // Smooth the track using Catmull-Rom spline
        trackPoints = CatmullRomSpline();

        // Create the mesh for the track surface
        meshFilter.mesh = CreateRacetrackMesh(trackPoints);
        gameObject.AddComponent<MeshCollider>();
    }

    void ResetGen()
    {
        Debug.Log("Reset");
        DestroyImmediate(gameObject.GetComponent<MeshFilter>());
        DestroyImmediate(gameObject.GetComponent<MeshRenderer>());

        meshFilter = null;
        meshRenderer = null;

        StartGen();
    }

    void GenerateSprintTrackPoints()
    {
        trackPoints.Clear();

        float currentX = 0f;
        float currentY = 0f;
        float currentZ = 0f;

        trackPoints.Add(new Vector3(currentX, currentY, currentZ)); // Add the starting point (0, 0, 0)

        for (int i = 1; i < trackLength; i++)
        {
            // Generate the directions using Perlin noise 
            //currentX = Mathf.PerlinNoise(i * seed + currentX, seed + currentX);
            currentZ = Mathf.PerlinNoise(i * seed + currentZ, seed + currentZ);
            currentY = Mathf.PerlinNoise(seed * currentY, 0.01f * currentY + seed) * 5f;

            currentX += Mathf.Cos(currentX * Mathf.PI * 2f) * trackWidth * 3;
            currentZ += Mathf.Sin(currentZ * Mathf.PI * 2f) * trackWidth * perlinScaleZ;
            currentY += Mathf.Sin(currentX * 0.1f) * 20f * perlinScaleY;

            trackPoints.Add(new Vector3(currentX, currentY, currentZ));
        }
    }

    void GenerateCircuitTrackPoints()
    {
        trackPoints.Clear();

        float currentX = 0f;
        float currentY = 0f;
        float currentZ = 0f;

        trackPoints.Add(new Vector3(currentX, currentY, currentZ)); // Add the starting point (0, 0, 0)


    }

    Mesh CreateRacetrackMesh(List<Vector3> points)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> triangles = new List<int>();
        int vertexIndex = 0;

        float uvCoordX1 = (90f / 780f);
        float uvCoordX2 = (690f / 780f);

        Vector2[] textUVs = new Vector2[] { new Vector2(uvCoordX1, 0f), new Vector2(uvCoordX2, 0f), Vector2.zero, Vector2.right, 
            new Vector2(uvCoordX1, 0.25f), new Vector2(uvCoordX2, 0.25f), new Vector2(0f, 0.25f), new Vector2(1f, 0.25f),
            new Vector2(uvCoordX1, 0.5f), new Vector2(uvCoordX2, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(uvCoordX1, 0.75f), new Vector2(uvCoordX2, 0.75f), new Vector2(0f, 0.75f), new Vector2(1f, 0.75f),
            new Vector2(uvCoordX1, 1f), new Vector2(uvCoordX2, 1f), Vector2.up, Vector2.one};

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Count - 1)
            {
                forward += points[i + 1] - points[i];
            }
            else
            {
                forward += points[i] - points[i - 1];
            }
            forward.Normalize();

            Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

            if (i < 2)
            {
                vertices.Add(points[i] + left * trackWidth * 0.5f);
                vertices.Add(points[i] - left * trackWidth * 0.5f);
            }
            else
            {
                Vector3 oldForward = points[i - 1] - points[i - 2];
                float angle = Vector3.SignedAngle(oldForward, forward, Vector3.up);

                float shift = Mathf.Lerp(0f, 1f, (angle + 90) / 180);

                vertices.Add(points[i] + left * trackWidth * shift);
                vertices.Add(points[i] - left * trackWidth * (1 - shift));
            }

            Vector3 a = vertices[vertices.Count - 2];
            a.y += 3f;
            Vector3 b = vertices[vertices.Count - 1];
            b.y += 3f;

            vertices.Add(a);
            vertices.Add(b);

            if (i % 5 == 0)
            {
                uvs.AddRange(textUVs);
            }

            if (i < points.Count - 1)
            {
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 4);
                triangles.Add(vertexIndex + 1);

                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 4);
                triangles.Add(vertexIndex + 5);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 4);

                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 6);
                triangles.Add(vertexIndex + 4);

                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 5);

                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 5);
                triangles.Add(vertexIndex + 7);

                Debug.DrawLine(points[i], points[i + 1], new UnityEngine.Color(1, 0, 0), 1000);
            }
            vertexIndex += 4;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // Catmull-Rom Spline function to smooth the path
    List<Vector3> CatmullRomSpline()
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