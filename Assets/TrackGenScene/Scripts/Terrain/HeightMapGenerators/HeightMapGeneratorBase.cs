using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeightMapGeneratorBase : MonoBehaviour
{
    public virtual Mesh GenerateHeightMap(float trackWidth, Vector2 sectorCoords, List<Vector3> trackPoints, float heightMarkipiler, int sectorSize, int resolution, int seed)
    {
        Mesh mesh = new Mesh();
        return mesh;
    }
}
