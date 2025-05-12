using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    public GameObject sectorPrefab;
    public Transform trackCenter;
    public int sectorSize = 64;
    public int sectorResolution = 16;

    public List<GameObject> instantiatedSectors = new List<GameObject>();

    public void Start()
    {
        
    }

    public void GenerateTerrain(int seed, float trackWidth, List<Vector3> trackPoints)
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

        int step = sectorSize - (sectorSize / sectorResolution);

        for (int z = (minZ - 100); z < (maxZ + 100); z += step)
        {
            for (int x = (minX - 100); x < (maxX + 100); x += step)
            {
                Vector2 coords = new Vector2(x, z);
                float distance = 10000;

                for (int i = 0; i < trackPoints.Count; i++) 
                {
                    Vector2 point1 = new Vector2(trackPoints[i].x, trackPoints[i].z);
                    Vector2 point2 = new Vector2(coords.x + (step / 2), coords.y + (step / 2));
                    if (Vector2.Distance(point1, point2) < distance) distance = Vector2.Distance(point1, point2);
                }

                if (distance < 100)
                {
                    GameObject sectorObj = Instantiate(sectorPrefab, Vector3.zero, Quaternion.identity, transform);
                    TerrainSector sector = sectorObj.GetComponent<TerrainSector>();
                    sector.sectorCoords = coords;
                    sector.GenerateHeightmap(trackWidth, trackPoints, sectorSize, sectorResolution, seed);

                    instantiatedSectors.Add(sectorObj);
                    sectorObj.transform.SetParent(transform);
                    sectorObj.transform.localPosition = Vector3.zero; //so that multiple terrains can be generated at the same time
                }
            }
        }
    }

    public void DeleteTerrain()
    {
        foreach(GameObject gayobject in instantiatedSectors)
            Destroy(gayobject);

        instantiatedSectors.Clear();
    }
}
