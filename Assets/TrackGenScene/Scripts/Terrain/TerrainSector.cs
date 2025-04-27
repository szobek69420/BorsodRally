using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TerrainSector : MonoBehaviour
{
    public float heightMultiplier = 10f;
    public Vector2 sectorCoords;

    public void GenerateHeightmap(float trackWidth, List<Vector3> trackPoints, int sectorSize, int resolution, int seed)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(resolution) * (resolution)];
        int[] triangles = new int[(resolution) * (resolution) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * (resolution);

                float worldX = sectorCoords.x + ((float)x * sectorSize / resolution);
                float worldZ = sectorCoords.y + ((float)y * sectorSize / resolution);
                Vector3 point = new Vector3(worldX, 0, worldZ);
                float distanceFromTrack = 10000;
                float dis = 0;
                int index = 0;

                for (int j = 0; j < trackPoints.Count; j++) 
                {
                    dis = Vector3.Distance(point, trackPoints[j]);

                    if (distanceFromTrack > dis) { distanceFromTrack = dis; index = j; }
                }
                float noise = Mathf.PerlinNoise((worldX * 0.1f) + seed, (worldZ * 0.1f) + seed);
                float falloff = Mathf.Clamp01(distanceFromTrack / 100f);
                //float falloff = EvaluateFalloffFromTrack(distanceFromTrack);
                Debug.Log(falloff);
                float height = noise * heightMultiplier * falloff;

                if (distanceFromTrack < trackWidth * 1.5)
                {
                    height = trackPoints[index].y - 0.75f;
                }

                vertices[i] = new Vector3(worldX, height, worldZ);
                uvs[i] = new Vector2((float)x / (resolution - 1), (float)y / (resolution - 1));
            }
        }

        int tri = 0;
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = x + y * (resolution);

                triangles[tri++] = i;
                triangles[tri++] = i + resolution;
                triangles[tri++] = i + 1;

                triangles[tri++] = i + 1;
                triangles[tri++] = i + resolution;
                triangles[tri++] = i + resolution + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    float EvaluateFalloffFromTrack(float distance)
    {
        float falloffStart = 0f;
        float falloffEnd = 100f;  

        float t = Mathf.InverseLerp(falloffStart, falloffEnd, distance);
        return Mathf.Pow(t, 2); // adjust curve sharpness
    }
}

