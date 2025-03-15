using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    private class SegmentOrientation
    {
        public Vector3 position;
        public float pitch, yaw;
        
        public SegmentOrientation(Vector3 position, float pitch, float yaw)
        {
            this.position = position;
            this.pitch = pitch;
            this.yaw = yaw;
        }
    }

    private Transform player;
    [SerializeField] private MeshFilter mf;
    [SerializeField] private MeshFilter mf_terrain;
    [SerializeField] private MeshCollider mc;

    [SerializeField] private GameObject checkpointPrefab;
    [SerializeField] private GameObject palmPrefab;
    [SerializeField] private GameObject trackEndPrefab;

    private static float SEGMENT_ADVANCE = 10.0f;

    private static int SEGMENT_VERTEX_COUNT = 16;
    private static int SEGMENT_INDEX_COUNT = 30;
    private static Vector3[] SEGMENT_VERTEX_POSITIONS = new Vector3[]{
        new(-10,0,0), new(-0.25f,0,0), new(0.25f, 0,0), new(10,0,0),
        new(-10,1.2f,0), new(-10,0.4f,0), new(10,1.2f,0), new(10,0.4f,0)
    };
    private static float[] SEGMENT_VERTEX_UVS_X = new float[]{
        0.0625f, 0.2734375f, 0.2890625f, 0.5f,
        0, 0.0625f, 0.0625f, 0
    };
    private static int[] SEGMENT_INDICES = new int[]{
        1,0,9,8,9,0, 2,1,10,9,10,1, 3,2,11,10,11,2,
        5,4,13,12,13,4, 6,7,15,15,14,6
    };

    private static int SEGMENT_COLLIDER_VERTEX_COUNT = 8;
    private static int SEGMENT_COLLIDER_INDEX_COUNT = 18;
    private static Vector3[] SEGMENT_COLLIDER_VERTEX_POSITIONS = new Vector3[]{
        new(-10,10,0),new(-10,0,0),new(10,0,0),new(10,10,0)
    };
    private static int[] SEGMENT_COLLIDER_INDICES = new int[]{
        1,0,5,4,5,0, 2,1,6,5,6,1, 3,2,7,6,7,2
    };

    private static int SEGMENT_TERRAIN_VERTEX_COUNT = 36;
    private static int SEGMENT_TERRAIN_INDEX_COUNT = 96;
    private static Vector3[] SEGMENT_TERRAIN_VERTEX_POSITIONS = new Vector3[] {
        new Vector3(-78, 10.0f,0), new Vector3(-72,13.0f,0), new Vector3(-64,19.0f,0), new Vector3(-56,22.0f,0), new Vector3(-52,20.0f,0), new Vector3(-46,8.0f,0),new Vector3(-38,3.0f,0), new Vector3(-26,0,0), new Vector3(-10,0,0),
        new Vector3(10,0,0), new Vector3(26,0,0), new Vector3(38,3.0f,0), new Vector3(46,8.0f,0), new Vector3(52,20.0f,0), new Vector3(56,22.0f,0),new Vector3(64,19.0f,0), new Vector3(72,13.0f,0), new Vector3(78, 10.0f,0)
    };
    private static int[] SEGMENT_TERRAIN_INDICES = new int[]{
        1,0,19,18,19,0, 2,1,20,19,20,1, 3,2,21,20,21,2, 4,3,22,21,22,3, 5,4,23,22,23,4, 6,5,24,23,24,5, 7,6,25,24,25,6, 8,7,26,25,26,7,
        10,9,28,27,28,9, 11,10,29,28,29,10, 12,11,30,29,30,11, 13,12,31,30,31,12, 14,13,32,31,32,13, 15,14,33,32,33,14, 16,15,34,33,34,15, 17,16,35,34,35,16
    };
    private static float[] SEGMENT_TERRAIN_VERTEX_UVS_X = new float[]{
        1,0.75f,1,0.75f,1,0.75f,1,0.75f,0.5f,
        0.5f,0.75f,1,0.75f,1,0.75f,1,0.75f,1
    };

    private static float[] POSSIBLE_PALM_POSITION_X = new float[] { 13.0f, 25.0f };
    private static float PALM_PROBABLITY = 0.1f;

    private const int MAX_SEGMENT_COUNT = 50;
    private const float SEGMENT_UNLOAD_DISTANCE = 50.0f;


    private SegmentOrientation[] segmentPositions;


    private Vector3[] segmentVertices;
    private int[] segmentIndices;
    private Vector2[] segmentUVs;

    private Vector3[] segmentColliderVertices;
    private int[] segmentColliderIndices;

    private Vector3[] segmentTerrainVertices;
    private int[] segmentTerrainIndices;
    private Vector2[] segmentTerrainUVs;


    private GameObject[] checkpoints=null;
    private int firstSegment;
    private int loadedSegments;

    private int nextCheckpointIndex;

    private Mesh trackMesh;
    private Mesh colliderMesh;
    private Mesh terrainMesh;

    private float currentTrackYaw = 50.0f;
    private float[] seed = new float[2];

    private bool isGenerating = false;

    private class PalmInstance
    {
        public GameObject palm;
        public int segmentIndex;
        public PalmInstance(GameObject palm, int segmentIndex)
        {
            this.palm = palm;
            this.segmentIndex = segmentIndex;
        }
    }
    private Queue<PalmInstance> palmInstances = new Queue<PalmInstance>();

    private GameObject trackEnd = null;

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
        seed[0] = Random.Range(0.0f, 10.0f);
        seed[1] = Random.Range(0.0f, 10.0f);
        this.player.localRotation = Quaternion.Euler(0.0f, currentTrackYaw, 0.0f);
        this.player.GetComponent<Rigidbody>().velocity = 10.0f * this.player.transform.forward;

        segmentPositions = new SegmentOrientation[MAX_SEGMENT_COUNT];
        segmentPositions[MAX_SEGMENT_COUNT - 1] = new SegmentOrientation(Vector3.zero, 0.0f, 0.0f);//because GenerateNewSegment doesn't check whether there is an existing SegmentOrientation at lastSegmentIndex

        segmentVertices = new Vector3[SEGMENT_VERTEX_COUNT * MAX_SEGMENT_COUNT];
        segmentIndices = new int[SEGMENT_INDEX_COUNT * MAX_SEGMENT_COUNT];
        segmentUVs = new Vector2[SEGMENT_VERTEX_COUNT * MAX_SEGMENT_COUNT];

        segmentColliderVertices = new Vector3[SEGMENT_COLLIDER_VERTEX_COUNT * MAX_SEGMENT_COUNT];
        segmentColliderIndices = new int[SEGMENT_COLLIDER_INDEX_COUNT * MAX_SEGMENT_COUNT];

        segmentTerrainVertices = new Vector3[SEGMENT_TERRAIN_VERTEX_COUNT * MAX_SEGMENT_COUNT];
        segmentTerrainIndices = new int[SEGMENT_TERRAIN_INDEX_COUNT * MAX_SEGMENT_COUNT];
        segmentTerrainUVs = new Vector2[SEGMENT_TERRAIN_VERTEX_COUNT * MAX_SEGMENT_COUNT];

        firstSegment = 0;
        loadedSegments = 0;
        nextCheckpointIndex = 0;

        trackMesh = new Mesh();
        colliderMesh= new Mesh();
        terrainMesh= new Mesh();

        while(palmInstances.Count > 0)//destroy remaining palm instances
        {
            GameObject.Destroy(palmInstances.Peek().palm);
            palmInstances.Dequeue();
        }

        //fill up the segment- and segment collider indices
        for(
            int i=0, currentSegmentVertex=0, currentSegmentColliderVertex=0, currentSegmentTerrainVertex=0;
            i<MAX_SEGMENT_COUNT;
            i++, currentSegmentVertex+=SEGMENT_VERTEX_COUNT, currentSegmentColliderVertex+=SEGMENT_COLLIDER_VERTEX_COUNT, currentSegmentTerrainVertex+=SEGMENT_TERRAIN_VERTEX_COUNT
            )
        {
            for(int j=0, startIndex = i * SEGMENT_INDEX_COUNT; j<SEGMENT_INDEX_COUNT; j++)
            {
                segmentIndices[startIndex + j] = SEGMENT_INDICES[j] + currentSegmentVertex;
            }

            for (int j = 0, startIndex = i * SEGMENT_COLLIDER_INDEX_COUNT; j < SEGMENT_COLLIDER_INDEX_COUNT; j++)
            {
                segmentColliderIndices[startIndex + j] = SEGMENT_COLLIDER_INDICES[j] + currentSegmentColliderVertex;
            }

            for (int j = 0, startIndex = i * SEGMENT_TERRAIN_INDEX_COUNT; j < SEGMENT_TERRAIN_INDEX_COUNT; j++)
            {
                segmentTerrainIndices[startIndex + j] = SEGMENT_TERRAIN_INDICES[j] + currentSegmentTerrainVertex;
            }
        }

        //instantiate checkpoints if necessary
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

        //instantiate track end if necessary
        if (trackEnd == null)
            trackEnd = Instantiate(trackEndPrefab, transform);



        for (int i = 0; i < MAX_SEGMENT_COUNT; i++)
            GenerateNewSegment();
        UpdateGPUAndColliderData();

        this.player.localPosition = segmentPositions[1].position + 2.0f*Vector3.up;
    }

    bool GenerateNewSegment() //returns true if the data has been modified
    {
        //check if one segment should be unloaded
        if (loadedSegments == MAX_SEGMENT_COUNT)
        {
            float distanceFromFirstSegment = (player.localPosition - segmentPositions[firstSegment].position).magnitude;

            if (distanceFromFirstSegment < SEGMENT_UNLOAD_DISTANCE)//load is unnecessary
                return false;

            //remove palms
            while(palmInstances.Count > 0&&palmInstances.Peek().segmentIndex == firstSegment)
            {
                GameObject.Destroy(palmInstances.Peek().palm);
                palmInstances.Dequeue();
            }

            firstSegment++;
            if (firstSegment == MAX_SEGMENT_COUNT)
                firstSegment = 0;

            loadedSegments--;
        }

        //load new segment
        int nextSegmentIndex = (firstSegment + loadedSegments) % MAX_SEGMENT_COUNT;
        int lastSegmentIndex = nextSegmentIndex == 0 ? MAX_SEGMENT_COUNT - 1 : nextSegmentIndex - 1;


        Vector3 forward = new(
            Mathf.Sin(currentTrackYaw * Mathf.Deg2Rad),
            0,
            Mathf.Cos(currentTrackYaw * Mathf.Deg2Rad));
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        for(int i=0;i<SEGMENT_VERTEX_COUNT/2;i++)
        {
            segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[lastSegmentIndex].position +
                SEGMENT_VERTEX_POSITIONS[i].x * right +
                SEGMENT_VERTEX_POSITIONS[i].y*Vector3.up;

            segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + i] = new Vector2(SEGMENT_VERTEX_UVS_X[i], 0.0f);
        }

        for (int i = 0; i < SEGMENT_COLLIDER_VERTEX_COUNT / 2; i++)
        {
            segmentColliderVertices[SEGMENT_COLLIDER_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[lastSegmentIndex].position +
                SEGMENT_COLLIDER_VERTEX_POSITIONS[i].x * right +
                SEGMENT_COLLIDER_VERTEX_POSITIONS[i].y * Vector3.up;
        }

        Random.InitState((int)(segmentPositions[lastSegmentIndex].position.x + segmentPositions[lastSegmentIndex].position.y));
        for (int i = 0; i < SEGMENT_TERRAIN_VERTEX_COUNT / 2; i++)
        {
            segmentTerrainVertices[SEGMENT_TERRAIN_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[lastSegmentIndex].position +
                SEGMENT_TERRAIN_VERTEX_POSITIONS[i].x * right +
                Random.Range(0.5f, 1.0f)*SEGMENT_TERRAIN_VERTEX_POSITIONS[i].y * Vector3.up;

            segmentTerrainUVs[SEGMENT_TERRAIN_VERTEX_COUNT * nextSegmentIndex + i] = new Vector2(SEGMENT_TERRAIN_VERTEX_UVS_X[i], 0.0f);
        }

        currentTrackYaw += 15.0f * Mathf.PerlinNoise(
            seed[0] +0.0047f * segmentPositions[lastSegmentIndex].position.x,
            seed[1] +0.007f * segmentPositions[lastSegmentIndex].position.z
            ) - 7.5f;


        forward = new Vector3(
            Mathf.Sin(currentTrackYaw * Mathf.Deg2Rad),
            0,
            Mathf.Cos(currentTrackYaw * Mathf.Deg2Rad));
        right = Vector3.Cross(Vector3.up, forward);

        segmentPositions[nextSegmentIndex] = new SegmentOrientation(
            segmentPositions[lastSegmentIndex].position + SEGMENT_ADVANCE * forward,
            0.0f,
            currentTrackYaw
            );

        for (int i = SEGMENT_VERTEX_COUNT/2, j=0; i < SEGMENT_VERTEX_COUNT; i++, j++)
        {
            segmentVertices[SEGMENT_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[nextSegmentIndex].position +
                SEGMENT_VERTEX_POSITIONS[j].x * right +
                SEGMENT_VERTEX_POSITIONS[j].y * Vector3.up;

            segmentUVs[SEGMENT_VERTEX_COUNT * nextSegmentIndex + i] = new Vector2(SEGMENT_VERTEX_UVS_X[j], 1.0f);
        }

        for (int i = SEGMENT_COLLIDER_VERTEX_COUNT/2, j=0; i < SEGMENT_COLLIDER_VERTEX_COUNT; i++, j++)
        {
            segmentColliderVertices[SEGMENT_COLLIDER_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[nextSegmentIndex].position +
                SEGMENT_COLLIDER_VERTEX_POSITIONS[j].x * right +
                SEGMENT_COLLIDER_VERTEX_POSITIONS[j].y * Vector3.up;
        }

        Random.InitState((int)(segmentPositions[nextSegmentIndex].position.x + segmentPositions[nextSegmentIndex].position.y));
        for (int i = SEGMENT_TERRAIN_VERTEX_COUNT / 2, j = 0; i < SEGMENT_TERRAIN_VERTEX_COUNT; i++, j++)
        {
            segmentTerrainVertices[SEGMENT_TERRAIN_VERTEX_COUNT * nextSegmentIndex + i] =
                segmentPositions[nextSegmentIndex].position +
                SEGMENT_TERRAIN_VERTEX_POSITIONS[j].x * right +
                Random.Range(0.5f, 1.0f)*SEGMENT_TERRAIN_VERTEX_POSITIONS[j].y * Vector3.up;

            segmentTerrainUVs[SEGMENT_TERRAIN_VERTEX_COUNT * nextSegmentIndex + i] = new Vector2(SEGMENT_TERRAIN_VERTEX_UVS_X[j], 1.0f);
        }

        //set the position of the checkpoint
        checkpoints[nextSegmentIndex].transform.localPosition = segmentPositions[nextSegmentIndex].position;
        checkpoints[nextSegmentIndex].transform.localRotation = Quaternion.Euler(0, currentTrackYaw, 0);
        checkpoints[nextSegmentIndex].SetActive(true);

        //generate palm
        if (Random.Range(0.0f, 1.0f) < PALM_PROBABLITY)//left side of the track
        {
            GameObject palm = GameObject.Instantiate(palmPrefab, transform);
            palm.transform.localPosition = segmentPositions[nextSegmentIndex].position + Random.Range(-POSSIBLE_PALM_POSITION_X[1], -POSSIBLE_PALM_POSITION_X[0]) * right;
            palm.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            palmInstances.Enqueue(new PalmInstance(palm, nextSegmentIndex));
        }
        if (Random.Range(0.0f, 1.0f)<PALM_PROBABLITY)//right side of the track
        {
            GameObject palm = GameObject.Instantiate(palmPrefab, transform);
            palm.transform.localPosition = segmentPositions[nextSegmentIndex].position + Random.Range(POSSIBLE_PALM_POSITION_X[0], POSSIBLE_PALM_POSITION_X[1]) * right;
            palm.transform.localRotation = Quaternion.Euler(0.0f, Random.Range(-180.0f, 180.0f), 0.0f);
            palmInstances.Enqueue(new PalmInstance(palm, nextSegmentIndex));
        }

        //move track end
        trackEnd.transform.localPosition = segmentPositions[firstSegment].position;
        trackEnd.transform.localRotation = Quaternion.Euler(segmentPositions[firstSegment].pitch, segmentPositions[firstSegment].yaw, 0.0f);

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

        colliderMesh.vertices = segmentColliderVertices;
        colliderMesh.triangles = segmentColliderIndices;
        colliderMesh.RecalculateBounds();
        colliderMesh.RecalculateNormals();
        mc.sharedMesh = colliderMesh;

        terrainMesh.vertices = segmentTerrainVertices;
        terrainMesh.triangles = segmentTerrainIndices;
        terrainMesh.uv = segmentTerrainUVs;
        terrainMesh.RecalculateBounds();
        terrainMesh.RecalculateNormals();
        mf_terrain.mesh = terrainMesh;
    }

    //an way for the player to notify the trackmanager about a reached checkpoint
    public void CheckpointReached(GameObject checkpoint)
    {
        for(int i=0;i<MAX_SEGMENT_COUNT;i++)
        {
            if (checkpoint == checkpoints[i])
            {
                checkpoints[i].SetActive(false);
                nextCheckpointIndex = i + 1;
                if (nextCheckpointIndex >= MAX_SEGMENT_COUNT)
                    nextCheckpointIndex = 0;
                break;
            }
        }
    }

    //it returns localPositions so that more tracks can be simulated simultaneously
    //the closest point of the checkpoint will be returned
    public Vector3 NextCheckpointPosition(Vector3 carWorldPosition)
    {
        Vector3 checkpointLeft = checkpoints[nextCheckpointIndex].GetComponent<Checkpoint>().leftEnd.position;
        Vector3 checkpointRight = checkpoints[nextCheckpointIndex].GetComponent<Checkpoint>().rightEnd.position;
        Vector3 checkpointLineDir = Vector3.Normalize(checkpointRight - checkpointLeft);

        //is beyond checkpointLeft?
        if (Vector3.Dot(checkpointLineDir, carWorldPosition - checkpointLeft) < 0)
            return transform.worldToLocalMatrix*checkpointLeft;

        //is beyond checkpointRight
        if (Vector3.Dot(checkpointLineDir, carWorldPosition - checkpointRight) > 0)
            return transform.worldToLocalMatrix * checkpointRight;

        return transform.worldToLocalMatrix * (Vector3.Dot(checkpointLineDir, carWorldPosition - checkpointLeft)*checkpointLineDir+checkpointLeft);
    }
}
