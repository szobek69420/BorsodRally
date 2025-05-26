using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainPopulatorBase : MonoBehaviour
{
    protected List<GameObject> instantiatedSectors;
    protected int sectorSize;
    protected int seed;

    public TerrainPopulatorBase(List<GameObject> instantiatedSectors, int sectorSize, int seed)
    {
        this.instantiatedSectors = instantiatedSectors;
        this.sectorSize = sectorSize;
        this.seed = seed;
    }

    public virtual void PopulateTerrain(float trackWidth, List<Vector3> trackPoints, List<PrefabConfig> prefabs, List<GameObject> environmentParts)
    {

    }

    protected float GetTerrainHeight(float x, float z)
    {
        foreach (GameObject sectorObj in instantiatedSectors)
        {
            TerrainSector sector = sectorObj.GetComponent<TerrainSector>();
            Vector2 sectorCoords = sector.sectorCoords;
            if (x >= sectorCoords.x && x < sectorCoords.x + sectorSize &&
                z >= sectorCoords.y && z < sectorCoords.y + sectorSize)
            {
                return sector.GetHeightAt(x - sectorCoords.x, z - sectorCoords.y);
            }
        }
        return 0f;
    }
}
