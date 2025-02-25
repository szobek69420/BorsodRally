using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    private Transform player;
    [SerializeField] private MeshFilter mf;
    [SerializeField] private MeshCollider mc;

    [SerializeField] private GameObject checkpointPrefab;

    private const int MAX_SEGMENT_COUNT = 50;
    private const float SEGMENT_UNLOAD_DISTANCE = 50.0f;
    private const float SEGMENT_LENGTH = 5.0f;

    private const int SEGMENT_VERTEX_COUNT = 8;
    private const int SEGMENT_INDEX_COUNT = 18;

    private Vector3[] segmentPositions;
    private Vector3[] segmentVertices;
    private int[] segmentIndices;
    private Vector2[] segmentUVs;
    private GameObject[] checkpoints=null;
    private int firstSegment;
    private int loadedSegments;
    private Mesh trackMesh;

    private float currentTrackYaw = 50.0f;

    private bool isGenerating = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        while (isGenerating&&GenerateNewSegment())
            UpdateGPUAndColliderData();
    }

    public void StartGenerate()
    {
        isGenerating = true;
    }

    public void EndGenerate()
    {
        isGenerating = false;
    }

    public void ResetTrack(Transform player)
    {
        this.player = player;

        currentTrackYaw = Random.Range(-180.0f, 180.0f);
        this.player.localRotation = Quaternion.Euler(0.0f, currentTrackYaw, 0.0f);

        segmentPositions = new Vector3[MAX_SEGMENT_COUNT];
        segmentVertices = new Vector3[SEGMENT_VERTEX_COUNT * MAX_SEGMENT_COUNT];
        segmentIndices = new int[SEGMENT_INDEX_COUNT * MAX_SEGMENT_COUNT];
        segmentUVs = new Vector2[SEGMENT_VERTEX_COUNT * MAX_SEGMENT_COUNT];
        firstSegment = 0;
        loadedSegments = 0;

        trackMesh = new Mesh();

        if(checkpoints!=null)
        {
            for (int i = 0; i < MAX_SEGMENT_COUNT; i++)
                checkpoints[i].transform.localPosition = new Vector3(0, 1000, 0);
        }
        else
        {
            checkpoints = new GameObject[MAX_SEGMENT_COUNT];
            for (int i = 0; i < MAX_SEGMENT_COUNT; i++)
            {
                checkpoints[i] = GameObject.Instantiate(
                    checkpointPrefab,
                    transform
                    );
                checkpoints[i].transform.localPosition = new Vector3(0, 1000, 0);
            }
        }

        for (int i = 0; i < MAX_SEGMENT_COUNT; i++)
            GenerateNewSegment();
        UpdateGPUAndColliderData();

        this.player.localPosition = segmentPositions[1] + 2.0f*Vector3.up;
    }

    bool GenerateNewSegment() //returns true if the data has been modified
    {
        //check if one segment should be unloaded
        if (loadedSegments == MAX_SEGMENT_COUNT)
        {
            float distanceFromFirstSegment = (player.localPosition - segmentPositions[firstSegment]).magnitude;

            if (distanceFromFirstSegment < SEGMENT_UNLOAD_DISTANCE)//load is unnecessary
                return false;

            firstSegment++;
            if (firstSegment == MAX_SEGMENT_COUNT)
                firstSegment = 0;

            loadedSegments--;
        }

        //load new segment
        int nextSegmentIndex = (firstSegment + loadedSegments) % MAX_SEGMENT_COUNT;
        int lastSegmentIndex = nextSegmentIndex == 0 ? MAX_SEGMENT_COUNT - 1 : nextSegmentIndex - 1;


        Vector3 forward = new Vector3(
            Mathf.Sin(currentTrackYaw * Mathf.Deg2Rad),
            0,
            Mathf.Cos(currentTrackYaw * Mathf.Deg2Rad));
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex] = segmentPositions[lastSegmentIndex] - 10 * right + 5 * Vector3.up;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 1] = segmentPositions[lastSegmentIndex] - 10 * right;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 2] = segmentPositions[lastSegmentIndex] + 10 * right;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 3] = segmentPositions[lastSegmentIndex] + 10 * right + 5 * Vector3.up;

        currentTrackYaw += 15.0f * Mathf.PerlinNoise(
            0.0047f * segmentPositions[lastSegmentIndex].x,
            0.007f * segmentPositions[lastSegmentIndex].z
            ) - 7.5f;

        forward = new Vector3(
            Mathf.Sin(currentTrackYaw * Mathf.Deg2Rad),
            0,
            Mathf.Cos(currentTrackYaw * Mathf.Deg2Rad));
        right = Vector3.Cross(Vector3.up, forward);

        segmentPositions[nextSegmentIndex] = segmentPositions[lastSegmentIndex] + SEGMENT_LENGTH * forward;

        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 4] = segmentPositions[nextSegmentIndex] - 10 * right + 5 * Vector3.up;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 5] = segmentPositions[nextSegmentIndex] - 10 * right;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 6] = segmentPositions[nextSegmentIndex] + 10 * right;
        segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 7] = segmentPositions[nextSegmentIndex] + 10 * right + 5 * Vector3.up;

        int segmentStartIndex = SEGMENT_VERTEX_COUNT * nextSegmentIndex;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex] = segmentStartIndex;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 1] = segmentStartIndex + 5;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 2] = segmentStartIndex + 1;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 3] = segmentStartIndex + 4;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 4] = segmentStartIndex + 5;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 5] = segmentStartIndex;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 6] = segmentStartIndex + 1;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 7] = segmentStartIndex + 6;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 8] = segmentStartIndex + 2;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 9] = segmentStartIndex + 5;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 10] = segmentStartIndex + 6;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 11] = segmentStartIndex + 1;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 12] = segmentStartIndex + 2;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 13] = segmentStartIndex + 7;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 14] = segmentStartIndex + 3;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 15] = segmentStartIndex + 6;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 16] = segmentStartIndex + 7;
        segmentIndices[SEGMENT_INDEX_COUNT * nextSegmentIndex + 17] = segmentStartIndex + 2;

        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex] = new Vector2(0.0f, 0.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 1] = new Vector2(0.1875f, 0.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 2] = new Vector2(0.8125f, 0.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 3] = new Vector2(1.0f, 0.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 4] = new Vector2(0.0f, 1.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 5] = new Vector2(0.1875f, 1.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 6] = new Vector2(0.8125f, 1.0f);
        segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + 7] = new Vector2(1.0f, 1.0f);

        //set the position of the checkpoint
        checkpoints[nextSegmentIndex].transform.localPosition = segmentPositions[nextSegmentIndex];
        checkpoints[nextSegmentIndex].transform.localRotation = Quaternion.Euler(0, currentTrackYaw, 0);
        checkpoints[nextSegmentIndex].SetActive(true);

        loadedSegments++;

        return true;
    }


    void UpdateGPUAndColliderData()
    {
        trackMesh.vertices = segmentVertices;
        trackMesh.triangles = segmentIndices;
        trackMesh.uv = segmentUVs;
        trackMesh.RecalculateBounds();
        trackMesh.RecalculateNormals();

        mf.mesh = trackMesh;
        mc.sharedMesh = trackMesh;
    }
}
