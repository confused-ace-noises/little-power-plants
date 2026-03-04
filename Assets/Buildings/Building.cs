using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Building", menuName = "2D/Bulding")]
public class Building : ScriptableObject
{
    public Vector2Int cells;
    public GameObject prefab;
    public Material standardMaterial;

    public BuildingBehavior buildingBehavior;
}
