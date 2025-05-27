using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinHeightMapGenerator : HeightMapGeneratorBase
{
    public override Mesh GenerateHeightMap(float trackWidth, Vector2 sectorCoords, List<Vector3> trackPoints, float heightMultiplier, int sectorSize, int resolution, int seed)
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
                    Vector3 point2 = trackPoints[j];
                    point2.y = 0;
                    dis = Vector3.Distance(point, point2);

                    if (distanceFromTrack > dis) { distanceFromTrack = dis; index = j; }
                }

                float noise = Mathf.PerlinNoise((worldX * 0.1f) + seed, (worldZ * 0.1f) + seed);
                float falloff = EvaluateFalloffFromTrack(distanceFromTrack);
                point.y = noise * heightMultiplier * falloff;

                if (distanceFromTrack < trackWidth * 3)
                {
                    float targetHeight = trackPoints[index].y - 1f;
                    float t = Mathf.InverseLerp(trackWidth * 3f, trackWidth / 2, distanceFromTrack);

                    if (distanceFromTrack < trackWidth * 1.1f)
                    {
                        point.y = targetHeight;
                    }
                }

                vertices[i] = point;
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
        return mesh;
    }

    float EvaluateFalloffFromTrack(float distance)
    {
        float maxDist = 100f;
        float t = Mathf.Clamp01(distance / maxDist);
        return Mathf.SmoothStep(0.2f, 1f, t);
    }
}
