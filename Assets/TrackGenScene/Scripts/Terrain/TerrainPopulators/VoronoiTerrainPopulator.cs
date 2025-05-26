using System.Collections.Generic;
using UnityEngine;

public class VoronoiTerrainPopulator : TerrainPopulatorBase
{
    public VoronoiTerrainPopulator(List<GameObject> instantiatedSectors, int sectorSize, int seed) : base(instantiatedSectors, sectorSize, seed)
    {
    }

    public override void PopulateTerrain(float trackWidth, List<Vector3> trackPoints, List<PrefabConfig> prefabs, List<GameObject> environmentParts)
    {   
        // Voronoi parameters
        int seedPointCount = 100; // Number of Voronoi seed points
        int maxSpectatorsPerCell = 3; // Max spectators per Voronoi cell
        float treeDensity = 0.1f; // Base tree density per cell

        // Calculate terrain bounds
        float minX = float.MaxValue, maxX = float.MinValue, minZ = float.MaxValue, maxZ = float.MinValue;
        foreach (GameObject sectorObj in instantiatedSectors)
        {
            Vector2 coords = sectorObj.GetComponent<TerrainSector>().sectorCoords;
            minX = Mathf.Min(minX, coords.x);
            maxX = Mathf.Max(maxX, coords.x + sectorSize);
            minZ = Mathf.Min(minZ, coords.y);
            maxZ = Mathf.Max(maxZ, coords.y + sectorSize);
        }

        Random.InitState(seed);
        List<Vector2> seedPoints = new List<Vector2>();
        for (int i = 0; i < seedPointCount; i++)
        {
            float x = Random.Range(minX, maxX);
            float z = Random.Range(minZ, maxZ);
            seedPoints.Add(new Vector2(x, z));
        }

        foreach (GameObject sectorObj in instantiatedSectors)
        {
            TerrainSector sector = sectorObj.GetComponent<TerrainSector>();
            Vector2 sectorCoords = sector.sectorCoords;

            for (int x = 0; x < sectorSize; x += 5)
            {
                for (int z = 0; z < sectorSize; z += 5)
                {
                    float worldX = sectorCoords.x + x;
                    float worldZ = sectorCoords.y + z;
                    Vector2 point = new Vector2(worldX, worldZ);

                    int nearestSeedIndex = 0;
                    float minDistance = float.MaxValue;
                    for (int i = 0; i < seedPoints.Count; i++)
                    {
                        float distance = Vector2.Distance(point, seedPoints[i]);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            nearestSeedIndex = i;
                        }
                    }

                    float trackDistance = float.MaxValue;
                    Vector3 closestTrackPoint = Vector3.zero;
                    foreach (Vector3 trackPoint in trackPoints)
                    {
                        float distance = Vector2.Distance(point, new Vector2(trackPoint.x, trackPoint.z));
                        if (distance < trackDistance)
                        {
                            trackDistance = distance;
                            closestTrackPoint = trackPoint;
                        }
                    }

                    Vector3 position = new Vector3(worldX, GetTerrainHeight(worldX, worldZ), worldZ);

                    if (trackDistance > trackWidth)
                    {
                        foreach (PrefabConfig prefab in prefabs)
                        {
                            if (trackDistance > prefab.minDistanceTrack && trackDistance < prefab.maxDistanceTrack
                                && Random.value < prefab.chance)
                            {
                                Quaternion rotation = Quaternion.LookRotation(position - closestTrackPoint, Vector3.up);
                                GameObject envPart = Instantiate(prefab.prefab, position, rotation);
                                envPart.transform.parent = GameObject.Find("TerrainManager").transform;
                                environmentParts.Add(envPart);
                            }
                        }
                        /*if (trackDistance < spectatorMaxDistance && Random.value < 0.1f)
                        {
                            int spectatorCount = Random.Range(1, maxSpectatorsPerCell + 1);
                            for (int j = 0; j < spectatorCount; j++)
                            {
                                Vector3 spectatorPos = position + new Vector3(
                                    Random.Range(-2f, 2f),
                                    0,
                                    Random.Range(-2f, 2f)
                                );
                                spectatorPos.y = GetTerrainHeight(spectatorPos.x, spectatorPos.z);

                                if (IsPositionValid(spectatorPos, trackPoints, trackWidth))
                                {
                                    Quaternion rotation = Quaternion.LookRotation(spectatorPos - closestTrackPoint);
                                    GameObject spectator = Instantiate(prefabs[0].prefab, spectatorPos, rotation);
                                    environmentParts.Add(spectator);
                                }
                            }
                        }
                        else if (trackDistance > treeMinDistance && trackDistance < treeMaxDistance)
                        {
                            float density = treeDensity * Mathf.PerlinNoise(worldX * 0.05f, worldZ * 0.05f);
                            if (Random.value < density)
                            {
                                GameObject tree = Instantiate(prefabs[1].prefab, position, Quaternion.identity);
                                environmentParts.Add(tree);
                            }
                        }*/
                    }
                }
            }
        }
    }
}
