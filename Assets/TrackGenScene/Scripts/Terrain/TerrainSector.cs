using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainSector : MonoBehaviour
{
    public int resolution = 16;
    public float size = 64f;
    public float heightMultiplier = 5f;
    public Vector2 sectorCoords;

    public void GenerateHeightmap(Vector3 trackPosition)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;

                float worldX = sectorCoords.x * size + ((float)x / (resolution - 1)) * size;
                float worldZ = sectorCoords.y * size + ((float)y / (resolution - 1)) * size;
                float distanceFromTrack = Vector3.Distance(new Vector3(worldX, 0, worldZ), trackPosition);

                float noise = Mathf.PerlinNoise(worldX * 0.05f, worldZ * 0.05f);
                float falloff = Mathf.Clamp01(distanceFromTrack / 100f);
                float height = noise * heightMultiplier * falloff * 0;

                vertices[i] = new Vector3(worldX, height, worldZ);
                uvs[i] = new Vector2((float)x / (resolution - 1), (float)y / (resolution - 1));
            }
        }

        int tri = 0;
        for (int y = 0; y < resolution - 1; y++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int i = x + y * resolution;

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
        //GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}

