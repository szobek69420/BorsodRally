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
                                int countPerSector = Random.Range(1, (int)prefab.bonusVariable + 1);
                                for (int j = 0; j < countPerSector; j++)
                                {
                                    Vector3 pos = position + new Vector3(
                                        Random.Range(-2f, 2f),
                                        0,
                                        Random.Range(-2f, 2f)
                                    );
                                    pos.y = GetTerrainHeight(pos.x, pos.z);

                                    if (prefab.rotate)
                                    {
                                        Quaternion rotation = Quaternion.LookRotation(position - closestTrackPoint, Vector3.up);
                                        GameObject envPart = Instantiate(prefab.prefab, position, rotation);
                                        envPart.transform.parent = GameObject.Find("TerrainManager").transform;
                                        environmentParts.Add(envPart);
                                    }
                                    else
                                    {
                                        GameObject envPart = Instantiate(prefab.prefab, position, Quaternion.identity);
                                        envPart.transform.parent = GameObject.Find("TerrainManager").transform;
                                        environmentParts.Add(envPart);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
