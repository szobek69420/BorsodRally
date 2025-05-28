using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiamondSquareHeightMapGenerator : HeightMapGeneratorBase
{
    public override Mesh GenerateHeightMap(float trackWidth, Vector2 sectorCoords, List<Vector3> trackPoints, float heightMultiplier, int sectorSize, int resolution, int seed)
    {
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[resolution * resolution];
        int[] triangles = new int[resolution * resolution * 6];
        Vector2[] uvs = new Vector2[vertices.Length];

        float[,] densityMap = GenerateDiamondSquare(resolution, seed, 0.5f, 0.3f);

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = x + y * resolution;

                float worldX = sectorCoords.x + ((float)x * sectorSize / resolution);
                float worldZ = sectorCoords.y + ((float)y * sectorSize / resolution);
                Vector3 point = new Vector3(worldX, 0, worldZ);
                float distanceFromTrack = 10000;
                int index = 0;

                for (int j = 0; j < trackPoints.Count; j++)
                {
                    Vector3 point2 = trackPoints[j];
                    point2.y = 0;
                    float dis = Vector3.Distance(point, point2);
                    if (distanceFromTrack > dis)
                    {
                        distanceFromTrack = dis;
                        index = j;
                    }
                }

                float noise = Mathf.PerlinNoise((worldX * 0.1f) + seed, (worldZ * 0.1f) + seed);
                float falloff = EvaluateFalloffFromTrack(distanceFromTrack);
                point.y = noise * heightMultiplier * 1.3f * falloff;

                if (distanceFromTrack < trackWidth * 3)
                {
                    float targetHeight = trackPoints[index].y - 1f;
                    float t = Mathf.InverseLerp(trackWidth * 4, trackWidth * 1.1f, distanceFromTrack);
                    if (distanceFromTrack < trackWidth * 1.1f)
                    {
                        point.y = targetHeight;
                    }
                    else
                    {
                        point.y = Mathf.Lerp(point.y, targetHeight, t);
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
        return mesh;
    }

    private float EvaluateFalloffFromTrack(float distance)
    {
        return Mathf.Clamp01(1f - (distance / 100f));
    }

    private float[,] GenerateDiamondSquare(int size, int seed, float roughness, float initialVariance)
    {
        float[,] map = new float[size, size];
        Random.InitState(seed);

        map[0, 0] = Random.value * initialVariance;
        map[0, size - 1] = Random.value * initialVariance;
        map[size - 1, 0] = Random.value * initialVariance;
        map[size - 1, size - 1] = Random.value * initialVariance;

        float variance = initialVariance;
        int step = size - 1;

        while (step > 1)
        {
            int halfStep = step / 2;

            for (int x = halfStep; x < size; x += step)
            {
                for (int y = halfStep; y < size; y += step)
                {
                    float average = (
                        map[x - halfStep, y - halfStep] +
                        map[x - halfStep, y + halfStep] +
                        map[x + halfStep, y - halfStep] +
                        map[x + halfStep, y + halfStep]
                    ) / 4f;
                    map[x, y] = average + (Random.value - 0.5f) * variance;
                }
            }

            for (int x = 0; x < size; x += halfStep)
            {
                for (int y = (x + halfStep) % step; y < size; y += step)
                {
                    float sum = 0f;
                    int count = 0;
                    if (x >= halfStep) { sum += map[x - halfStep, y]; count++; }
                    if (x + halfStep < size) { sum += map[x + halfStep, y]; count++; }
                    if (y >= halfStep) { sum += map[x, y - halfStep]; count++; }
                    if (y + halfStep < size) { sum += map[x, y + halfStep]; count++; }
                    map[x, y] = sum / count + (Random.value - 0.5f) * variance;
                }
            }

            variance *= roughness;
            step /= 2;
        }

        float min = float.MaxValue, max = float.MinValue;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                min = Mathf.Min(min, map[x, y]);
                max = Mathf.Max(max, map[x, y]);
            }
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                map[x, y] = Mathf.InverseLerp(min, max, map[x, y]);

        return map;
    }

}
