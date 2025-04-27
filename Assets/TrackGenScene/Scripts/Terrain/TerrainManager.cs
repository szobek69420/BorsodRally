using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject sectorPrefab;
    public int gridSize = 5;
    public Transform trackCenter;
    public int sectorSize = 64;

    public void Start()
    {
        
    }

    public void GenerateTerrain(List<Vector3> trackPoints)
    {
        int minX = 10000;
        int maxX = -10000;
        int minZ = 10000;
        int maxZ = -10000;

        for (int i = 0; i < trackPoints.Count; i++)
        {
            if (trackPoints[i].x < minX) minX = (int)trackPoints[i].x;
            if (trackPoints[i].x > maxX) maxX = (int)trackPoints[i].x;
            if (trackPoints[i].z < minZ) minZ = (int)trackPoints[i].z;
            if (trackPoints[i].z > maxZ) maxZ = (int)trackPoints[i].z;
        }

        for (int z = (minZ - 100) / sectorSize; z <= (maxZ + 100) / sectorSize; z++)
        {
            for (int x = (minX - 100) / sectorSize; x <= (maxX + 100) / sectorSize; x++)
            {
                Vector2 coords = new Vector2(x, z);
                GameObject sectorObj = Instantiate(sectorPrefab, Vector3.zero, Quaternion.identity, transform);
                TerrainSector sector = sectorObj.GetComponent<TerrainSector>();
                sector.sectorCoords = coords;
                sector.GenerateHeightmap(trackCenter.position);
            }
        }
    }
}
