using UnityEngine;

[CreateAssetMenu(menuName = "MapDecor/New Decor")]
public class MapDecor : ScriptableObject
{
    public string Name;

    public int sizeX;
    public int sizeY;

    public bool requiresNetworking;

    public GameObject Prefab;
}
