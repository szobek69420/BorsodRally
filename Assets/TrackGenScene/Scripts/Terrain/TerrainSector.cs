using System.Collections.Generic;
using UnityEngine;

public class TerrainSector : MonoBehaviour
{
    public Vector2 sectorCoords;
    public int sectorSize;
    public int resolution;
    public float heightMultiplier = 10f;
    private HeightMapGeneratorBase heightMapGenerator;

    public void GenerateHeightmap(float trackWidth, List<Vector3> trackPoints, int sectSize, int res, int seed)
    {
        sectorSize = sectSize;
        resolution = res;
        
        //heightMapGenerator = new PerlinHeightMapGenerator();
        heightMapGenerator = new DiamondSquareHeightMapGenerator();

        GetComponent<MeshFilter>().mesh = heightMapGenerator.GenerateHeightMap(trackWidth, sectorCoords, trackPoints, heightMultiplier, sectorSize, resolution, seed);
    }

    public float GetHeightAt(float x, float z)
    {
        float localX = x / sectorSize * (resolution - 1);
        float localZ = z / sectorSize * (resolution - 1);
        int x0 = Mathf.FloorToInt(localX);
        int z0 = Mathf.FloorToInt(localZ);
        float fracX = localX - x0;
        float fracZ = localZ - z0;

        x0 = Mathf.Clamp(x0, 0, resolution - 2);
        z0 = Mathf.Clamp(z0, 0, resolution - 2);
        int x1 = x0 + 1;
        int z1 = z0 + 1;

        // Get heights from mesh
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        Vector3[] vertices = mesh.vertices;
        int i00 = x0 + z0 * resolution;
        int i10 = x1 + z0 * resolution;
        int i01 = x0 + z1 * resolution;
        int i11 = x1 + z1 * resolution;

        float h00 = vertices[i00].y;
        float h10 = vertices[i10].y;
        float h01 = vertices[i01].y;
        float h11 = vertices[i11].y;

        float h0 = Mathf.Lerp(h00, h10, fracX);
        float h1 = Mathf.Lerp(h01, h11, fracX);
        return Mathf.Lerp(h0, h1, fracZ);
    }
}

