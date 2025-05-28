using System.Collections.Generic;
using UnityEngine;

public class SimpleTerrainPopulator : TerrainPopulatorBase
{

    public SimpleTerrainPopulator(List<GameObject> instantiatedSectors, int sectorSize, int seed) : base(instantiatedSectors, sectorSize, seed)
    {
    }

    public override void PopulateTerrain(float trackWidth, List<Vector3> trackPoints, List<PrefabConfig> prefabs, List<GameObject> environmentParts)
    {
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
                                if (prefab.rotate)
                                {
                                    Quaternion rotation = Quaternion.LookRotation(position - closestTrackPoint, Vector3.up);
                                    GameObject envPart = Instantiate(prefab.prefab, position, rotation);
                                    envPart.transform.parent = GameObject.Find("TerrainManager").transform;
                                    environmentParts.Add(envPart);
                                }
                                else { 
                                    GameObject envPart = Instantiate(prefab.prefab, position, Quaternion.identity);
                                    envPart.transform.parent = GameObject.Find("TerrainManager").transform;
                                    environmentParts.Add(envPart);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
                
