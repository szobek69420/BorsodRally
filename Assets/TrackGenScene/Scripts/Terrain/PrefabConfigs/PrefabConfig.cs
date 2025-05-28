using UnityEngine;

[CreateAssetMenu(fileName = "PrefabConfig", menuName = "Terrain/PrefabConfig")]
public class PrefabConfig : ScriptableObject
{
    public GameObject prefab;
    public float minDistanceTrack = 0f;
    public float maxDistanceTrack = 100f;
    public float chance = 0.01f;
    public float bonusVariable = 0f;
    public bool rotate = false;
}
