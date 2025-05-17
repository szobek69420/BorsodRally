using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using System.Reflection;
using System.Drawing;
using System.Diagnostics;

public class RacetrackGenerator : MonoBehaviour
{
    [SerializeField] private int START_LINE_INDEX = 15;
    [SerializeField] private int FINISH_LINE_INDEX = 15;

    [SerializeField] private TerrainManager terrain;
    [SerializeField] private bool generateEnvironment = true;

    public int seed = 42;                                              // Seed for generation
    public int trackLength = 30;                                       // Number of track segments
    public int trackWidth = 15;                                        // Width of the track
    public float curviness = 4f;                                       // Scale for the curviness
    public float elevation = 5f;                                       // Scale for the elevation Y-axis 
    public Material trackMaterial;                                     // Material to apply to the track surface

    private int trackSectors = 20;
    private List<Vector3> trackPoints = new List<Vector3>();           // List to hold track control points
    private PathGeneratorBase controlPointGenerator;
    private List<GameObject> trackParts = new List<GameObject>();      // List to hold track sections
    private List<GameObject> trackWalls = new List<GameObject>();      // List to hold guide walls for ML cars 

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
        //if(Input.GetKeyDown(KeyCode.R)) { ResetGen(); }
    }

    public void FetchParameters()
    {
        int processId = Process.GetCurrentProcess().Id;

        seed = PlayerPrefs.GetInt("seed"+processId);
        trackLength = PlayerPrefs.GetInt("length"+processId);
        curviness = PlayerPrefs.GetFloat("curviness"+processId);
    }

    public void RandomizeParameters()
    {
        seed = UnityEngine.Random.Range(0, 200000);
        trackLength = 80;//this should be the same for every ml episode
        curviness = UnityEngine.Random.Range(5.0f, 10.0f);
    }

    //cannot set the ip and difficulty parameters
    public LobbyTrackInfo SerializeParameters()
    {
        return new LobbyTrackInfo(null, trackLength, seed, curviness, 69);
    }

    public void StartGen()
    {
        controlPointGenerator = new RandomPathGenerator();        //You can change the pathgenerator here
        terrain = GameObject.Find("TerrainManager").GetComponent<TerrainManager>();

        trackPoints.Clear();
        trackPoints = controlPointGenerator.GenerateTrackPoints(seed, trackLength, curviness, elevation);

        trackParts.Clear();
        trackWalls.Clear();

        for(int i = 0; i < trackSectors; i++)
        {
            trackParts.Add(new GameObject("Sector "+ (i + 1)));
            trackParts[i].transform.SetParent(gameObject.transform);
            trackParts[i].transform.localPosition= Vector3.zero;//so that more tracks can be generated simultaneously
            trackParts[i].layer = 6;                            //the track layer, necessary for the ml agents

            trackParts[i].AddComponent<MeshFilter>();
            trackParts[i].AddComponent<MeshRenderer>();
            trackParts[i].AddComponent<MeshCollider>();

            if (trackMaterial != null)
            {
                trackParts[i].GetComponent<MeshRenderer>().material = trackMaterial;
            }


            trackWalls.Add(new GameObject("Guide Wall " + (i + 1)));
            trackWalls[i].transform.SetParent(gameObject.transform);
            trackWalls[i].transform.localPosition = Vector3.zero;//so that more tracks can be generated simultaneously
            trackWalls[i].layer = 7;                            //the track layer, necessary for the ml agents

            trackWalls[i].AddComponent<MeshFilter>();
            trackWalls[i].AddComponent<MeshCollider>();
        }

        trackWidth += 2;
        CreateRacetrackMesh();                                  //Create the mesh for the track surface    
        trackWidth -= 2;
        CreateMLGuideMesh();                                    //Create mesh for track walls 

        if(generateEnvironment)
            terrain.GenerateTerrain(seed, trackWidth, trackPoints);                   //Terrain generating

        if (startLine != null)
            Destroy(startLine);
        startLine = new GameObject("StartLine");
        startLine.tag = "StartLine";
        startLine.transform.SetParent(gameObject.transform);
        startLine.AddComponent<BoxCollider>();
        Vector3 forw = Vector3.Normalize(trackPoints[START_LINE_INDEX] - trackPoints[START_LINE_INDEX - 1]);
        startLine.transform.rotation = Quaternion.LookRotation(forw);
        BoxCollider strL = startLine.GetComponent<BoxCollider>();
        strL.transform.localPosition = trackPoints[START_LINE_INDEX];
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
        fnshL.transform.localPosition = trackPoints[trackPoints.Count - FINISH_LINE_INDEX];
        fnshL.size = new Vector3(trackWidth, 10, 2);
        fnshL.isTrigger = true;
    }
        
    public void ResetGen()
    {
        //UnityEngine.Debug.Log("Reset");

        foreach(GameObject go in trackParts)
            Destroy(go);
        foreach (GameObject go in trackWalls)
            Destroy(go);

        terrain.DeleteTerrain();
        StartGen();
    }

    private void CreateRacetrackMesh()
    {
        int index = 0;
        int pointsInOneSector = trackPoints.Count / trackSectors;

        float uvCoordX1 = (90f / 780f);
        float uvCoordX2 = (690f / 780f);
        Vector2[] textUVs = new Vector2[] { new Vector2(uvCoordX1, 0f), new Vector2(uvCoordX2, 0f), Vector2.zero, Vector2.right,
            new Vector2(uvCoordX1, 1f), new Vector2(uvCoordX2, 1f), Vector2.up, Vector2.one};

        for (int i = 0; i < trackSectors; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
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

                if (step < trackPoints.Count - 1)
                {
                    forward += trackPoints[step + 1] - trackPoints[step];
                }
                else if(step < trackPoints.Count)
                {
                    forward += trackPoints[step] - trackPoints[step - 1];
                }
                forward.Normalize();

                Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

                if (step < 2)
                {
                    vertices.Add(trackPoints[step] + left * trackWidth * 0.5f);  
                    vertices.Add(trackPoints[step] - left * trackWidth * 0.5f);
                }
                else
                {
                    Vector3 oldForward = trackPoints[step - 1] - trackPoints[step - 2];
                    float angle = Vector3.SignedAngle(oldForward, forward, Vector3.up);

                    float shift = Mathf.Lerp(0f, 1f, (angle + 90) / 180);

                    vertices.Add(trackPoints[step] + left * trackWidth * shift);
                    vertices.Add(trackPoints[step] - left * trackWidth * (1 - shift));
                }

                Vector3 a = vertices[vertices.Count - 2];
                a.y += 5f;
                Vector3 b = vertices[vertices.Count - 1];
                b.y += 5f;

                vertices.Add(a);
                vertices.Add(b);

                if ((j + 1) % 2 == 0)
                {
                    uvs.AddRange(textUVs);
                }

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
                //if (step < trackPoints.Count - 1) UnityEngine.Debug.DrawLine(trackPoints[step], trackPoints[step + 1], new UnityEngine.Color(1, 0, 0), 1000);
                vertexIndex += 4;
            }

            if(vertices.Count % 8 != 0) { uvs.AddRange(textUVs[0..4]); }

            Mesh meshF = new Mesh();
            meshF.vertices = vertices.ToArray();
            meshF.triangles = triangles.ToArray();
            meshF.uv = uvs.ToArray();
            meshF.RecalculateNormals();
            meshF.RecalculateBounds();
            trackParts[index].GetComponent<MeshFilter>().mesh = meshF;
            trackParts[index++].GetComponent<MeshCollider>().sharedMesh = meshF;
        }
    }

    private void CreateMLGuideMesh()
    {
        int index = 0;
        int pointsInOneSector = trackPoints.Count / trackSectors;
        int wallHeight = 80;                                        //change the height of the wall in both directions

        for (int i = 0; i < trackSectors; i++)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            int vertexIndex = 0;
            int k = 0;

            Vector3 lt = new Vector3();
            Vector3 rt = new Vector3();

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

                if (step < trackPoints.Count - 1)
                {
                    forward += trackPoints[step + 1] - trackPoints[step];
                }
                else if (step < trackPoints.Count)
                {
                    forward += trackPoints[step] - trackPoints[step - 1];
                }
                forward.Normalize();

                Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

                if (step < 2)
                {
                    lt = (trackPoints[step] + left * trackWidth * 0.5f);
                    rt = (trackPoints[step] - left * trackWidth * 0.5f);
                }
                else
                {
                    Vector3 oldForward = trackPoints[step - 1] - trackPoints[step - 2];
                    float angle = Vector3.SignedAngle(oldForward, forward, Vector3.up);

                    float shift = Mathf.Lerp(0f, 1f, (angle + 90) / 180);

                    lt = (trackPoints[step] + left * trackWidth * shift);
                    rt = (trackPoints[step] - left * trackWidth * (1 - shift));
                }

                if (j > 0)
                {
                    Vector2 v1 = new Vector2(trackPoints[step - 1].x, trackPoints[step - 1].z);
                    Vector2 v2 = new Vector2(trackPoints[step].x, trackPoints[step].z);
                    Vector2 v3 = new Vector2(vertices[vertices.Count - 4].x, vertices[vertices.Count - 4].z);
                    Vector2 v4 = new Vector2(vertices[vertices.Count - 3].x, vertices[vertices.Count - 3].z);
                    Vector2 v5 = new Vector2(lt.x, lt.z);
                    Vector2 v6 = new Vector2(rt.x, rt.z);

                    if (DoIntersect(v1, v3, v2, v5)) lt = vertices[vertices.Count - 4];
                    if (DoIntersect(v1, v4, v2, v6)) rt = vertices[vertices.Count - 3];
                }

                lt.y += wallHeight;
                rt.y += wallHeight;

                vertices.Add(lt);
                vertices.Add(rt);

                lt.y -= 2 * wallHeight;
                rt.y -= 2 * wallHeight;

                vertices.Add(lt);
                vertices.Add(rt);

                if (j < k - 1)
                {
                    triangles.Add(vertexIndex);
                    triangles.Add(vertexIndex + 4);
                    triangles.Add(vertexIndex + 2);

                    triangles.Add(vertexIndex + 2);
                    triangles.Add(vertexIndex + 4);
                    triangles.Add(vertexIndex + 6);

                    triangles.Add(vertexIndex + 3);
                    triangles.Add(vertexIndex + 5);
                    triangles.Add(vertexIndex + 1);

                    triangles.Add(vertexIndex + 3);
                    triangles.Add(vertexIndex + 7);
                    triangles.Add(vertexIndex + 5);
                }
                vertexIndex += 4;
            }

            Mesh meshF = new Mesh();
            meshF.vertices = vertices.ToArray();
            meshF.triangles = triangles.ToArray();
            meshF.RecalculateNormals();
            meshF.RecalculateBounds();
            trackWalls[index].GetComponent<MeshFilter>().mesh = meshF;
            trackWalls[index++].GetComponent<MeshCollider>().sharedMesh = meshF;
        }
    }

    public static bool DoIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
    {
        int o1 = Orientation(p1, q1, p2);
        int o2 = Orientation(p1, q1, q2);
        int o3 = Orientation(p2, q2, p1);
        int o4 = Orientation(p2, q2, q1);

        if (o1 != o2 && o3 != o4)
        {
            return true;
        }

        if (o1 == 0 && OnSegment(p1, p2, q1)) return true;

        if (o2 == 0 && OnSegment(p1, q2, q1)) return true;

        if (o3 == 0 && OnSegment(p2, p1, q2)) return true;

        if (o4 == 0 && OnSegment(p2, q1, q2)) return true;

        return false;
    }

    private static int Orientation(Vector2 p, Vector2 q, Vector2 r)
    {
        float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);

        if (Mathf.Abs(val) < 1e-9f) return 0;

        return (val > 0) ? 1 : 2;
    }

    private static bool OnSegment(Vector2 p, Vector2 q, Vector2 r)
    {
        return (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y));
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