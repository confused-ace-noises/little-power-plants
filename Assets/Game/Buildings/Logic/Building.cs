using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "Building", menuName = "2D/Bulding")]
public class Building : ScriptableObject
{
    public Vector2Int cells;
    public GameObject prefab;
    public Material standardMaterial;

    public BuildingBehavior buildingBehavior;

    public Vector2Int[] OccupiedCells (Vector3Int pos)
    {
        Vector2Int[] occupiedCells = new Vector2Int[cells.x * cells.y];
        int index = 0;
        for (int x = 0; x < cells.x; x++)
        {
            for (int y = 0; y < cells.y; y++)
            {
                occupiedCells[index++] = new Vector2Int(pos.x + x, pos.y + y);
            }
        }

        return occupiedCells;
    }

    
}
