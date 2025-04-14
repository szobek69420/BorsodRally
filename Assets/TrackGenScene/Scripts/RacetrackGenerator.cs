using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;


public class RacetrackGenerator : MonoBehaviour
{
    private const int START_LINE_INDEX = 15;
    private const int FINISH_LINE_INDEX = 15;

    public int seed = 42;                                              // Seed for the Perlin Noise generation
    public int trackLength = 30;                                      // Number of track segments
    public int trackWidth = 15;                                        // Width of the track
    public float perlinScaleZ = 4f;                                    // Scale for the Perlin noise in Z-axis
    public float perlinScaleY = 1f;                                    // Scale for the Perlin noise in Y-axis 
    public Material trackMaterial;                                     // Material to apply to the track surface

    private int trackSectors = 20;
    private List<Vector3> trackPoints = new List<Vector3>();           // List to hold track control points
    
    private List<GameObject> trackParts = new List<GameObject>();      // List to hold track sections

    private GameObject startLine=null;
    private GameObject finishLine=null;

    public List<Vector3> TrackPoints { get { return trackPoints; } }

    private void Start()
    {
        //FetchParameters();
        //StartGen();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R)) { ResetGen(); }
    }

    public void FetchParameters()
    {
        int processId=Process.GetCurrentProcess().Id;

        seed = PlayerPrefs.GetInt("seed"+processId);
        trackLength = PlayerPrefs.GetInt("length"+processId);
        perlinScaleZ = PlayerPrefs.GetFloat("curviness"+processId);
    }

    //cannot set the ip and difficulty parameters
    public LobbyTrackInfo SerializeParameters()
    {
        return new LobbyTrackInfo(null, trackLength, seed, perlinScaleZ, 69);
    }

    public void StartGen()
    {
        UnityEngine.Random.InitState(seed);

        GenerateSprintTrackPoints();
        //GenerateCircuitTrackPoints();

        trackPoints = CatmullRomSpline();

        trackParts.Clear();

        for(int i = 0; i < trackSectors; i++)
        {
            trackParts.Add(new GameObject("Sector "+ (i + 1)));
            trackParts[i].transform.SetParent(gameObject.transform);
            trackParts[i].layer = 6;    //the track layer, necessary for the ml agents

            trackParts[i].AddComponent<MeshFilter>();
            trackParts[i].AddComponent<MeshRenderer>();
            trackParts[i].AddComponent<MeshCollider>();

            if (trackMaterial != null)
            {
                trackParts[i].GetComponent<MeshRenderer>().material = trackMaterial;
            }
        }

        // Create the mesh for the track surface
        CreateRacetrackPhysicsMesh(trackPoints, trackParts);
        //CreateRacetrackVisualMesh(trackPoints, trackParts);

        if (startLine != null)
            Destroy(startLine);
        startLine = new GameObject("StartLine");
        startLine.tag = "StartLine";
        startLine.transform.SetParent(gameObject.transform);
        startLine.AddComponent<BoxCollider>();
        Vector3 forw = Vector3.Normalize(trackPoints[START_LINE_INDEX] - trackPoints[START_LINE_INDEX - 1]);
        startLine.transform.rotation = Quaternion.LookRotation(forw);
        BoxCollider strL = startLine.GetComponent<BoxCollider>();
        strL.transform.position = trackPoints[START_LINE_INDEX];
        strL.size = new Vector3(trackWidth, 10, 2);
        strL.isTrigger = true;

        if (finishLine != null)
            Destroy(finishLine);
        finishLine = new GameObject("FinishLine");
        finishLine.tag = "FinishLine";
        finishLine.transform.SetParent(gameObject.transform);
        finishLine.AddComponent<BoxCollider>();
        forw = Vector3.Normalize(trackPoints[trackPoints.Count - FINISH_LINE_INDEX] - trackPoints[trackPoints.Count - FINISH_LINE_INDEX - 1]);
        finishLine.transform.rotation=Quaternion.LookRotation(forw);
        BoxCollider fnshL = finishLine.GetComponent<BoxCollider>();
        fnshL.transform.position = trackPoints[trackPoints.Count - FINISH_LINE_INDEX];
        fnshL.size = new Vector3(trackWidth, 10, 2);
        fnshL.isTrigger = true;

    }
        
    public void ResetGen()
    {
        UnityEngine.Debug.Log("Reset");

        foreach(GameObject go in trackParts)
            DestroyImmediate(go);

        StartGen();
    }

    private void GenerateSprintTrackPoints()
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
            currentY += Mathf.Sin(currentX * 0.1f) * 10f * perlinScaleY;

            trackPoints.Add(new Vector3(currentX, currentY, currentZ));
        }
    }

    private void GenerateCircuitTrackPoints()
    {
        trackPoints.Clear();

        float currentX = 0f;
        float currentY = 0f;
        float currentZ = 0f;

        trackPoints.Add(new Vector3(currentX, currentY, currentZ)); // Add the starting point (0, 0, 0)

    }

    private List<Vector3> CatmullRomSpline()
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

    private void CreateRacetrackPhysicsMesh(List<Vector3> points, List<GameObject> gameObj)
    {
        int index = 0;
        int pointsInOneSector = trackPoints.Count / trackSectors;

        for (int i = 0; i < trackSectors; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            int vertexIndex = 0;
            int k = 0;

            if (i == trackSectors - 1)
            {
                k = pointsInOneSector;
            }
            else
            {
                k = pointsInOneSector + 1;
            }

            for (int j = 0; j < k; j++)
            {
                Vector3 forward = Vector3.zero;
                int step = (i * pointsInOneSector) + j;

                if (step < points.Count - 1)
                {
                    forward += points[step + 1] - points[step];
                }
                else if(step < trackPoints.Count)
                {
                    forward += points[step] - points[step - 1];
                }
                forward.Normalize();

                Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

                if (step < 2)
                {
                    vertices.Add(points[step] + left * trackWidth * 0.5f);
                    vertices.Add(points[step] - left * trackWidth * 0.5f);
                }
                else
                {
                    Vector3 oldForward = points[step - 1] - points[step - 2];
                    float angle = Vector3.SignedAngle(oldForward, forward, Vector3.up);

                    float shift = Mathf.Lerp(0f, 1f, (angle + 90) / 180);

                    vertices.Add(points[step] + left * trackWidth * shift);
                    vertices.Add(points[step] - left * trackWidth * (1 - shift));
                }

                Vector3 a = vertices[vertices.Count - 2];
                a.y += 3f;
                Vector3 b = vertices[vertices.Count - 1];
                b.y += 3f;

                vertices.Add(a);
                vertices.Add(b);

                if (j < k - 1)
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
                }
                //if (step < points.Count - 1) Debug.DrawLine(points[step], points[step + 1], new UnityEngine.Color(1, 0, 0), 1000);
                vertexIndex += 4;
            }
            Mesh meshF = new Mesh();
            meshF.vertices = vertices.ToArray();
            meshF.triangles = triangles.ToArray();
            meshF.RecalculateNormals();
            meshF.RecalculateBounds();
            gameObj[index].GetComponent<MeshFilter>().mesh = meshF;  
            gameObj[index++].GetComponent<MeshCollider>().sharedMesh = meshF;
        }

    }

    private void CreateRacetrackVisualMesh(List<Vector3> points, List<GameObject> gameObj) 
    {
        float uvCoordX1 = (90f / 780f);
        float uvCoordX2 = (690f / 780f);

        Vector2[] textUVs = new Vector2[] { new Vector2(uvCoordX1, 0f), new Vector2(uvCoordX2, 0f), Vector2.zero, Vector2.right, 
            new Vector2(uvCoordX1, 0.25f), new Vector2(uvCoordX2, 0.25f), new Vector2(0f, 0.25f), new Vector2(1f, 0.25f),
            new Vector2(uvCoordX1, 0.5f), new Vector2(uvCoordX2, 0.5f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
            new Vector2(uvCoordX1, 0.75f), new Vector2(uvCoordX2, 0.75f), new Vector2(0f, 0.75f), new Vector2(1f, 0.75f),
            new Vector2(uvCoordX1, 1f), new Vector2(uvCoordX2, 1f), Vector2.up, Vector2.one};
    }

    public Transform GetStartLine()
    {
        return startLine?.transform;
    }

    public Transform GetFinishLine()
    {
        return finishLine?.transform;
    }

    //returns a value in [0;1] based on the closest trackpoint to the given checkpoint
    //the start and finish positions are considered
    public float CalculateProgress(Vector3 racistPosition)
    {
        int closestIndex = GetNearestTrackPointIndex(racistPosition);
        return (((float)(closestIndex-START_LINE_INDEX))/(trackPoints.Count-FINISH_LINE_INDEX-START_LINE_INDEX+1));
    }

    public int GetNearestTrackPointIndex(Vector3 racistPosition)
    {
        int closestIndex = -1;
        float minSqrDistance = float.PositiveInfinity;
        for (int i = START_LINE_INDEX; i <= trackPoints.Count - FINISH_LINE_INDEX; i++)
        {
            float distance = (racistPosition - trackPoints[i]).sqrMagnitude;
            if (distance < minSqrDistance)
            {
                minSqrDistance = distance;
                closestIndex = i;
            }
        }

        //check if there is gebasz
        if (closestIndex == -1)
            throw new Exception("There are no suitable track points");

        return closestIndex;
    }
}