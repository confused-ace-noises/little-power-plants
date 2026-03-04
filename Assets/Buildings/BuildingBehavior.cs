using UnityEngine;

// [System.Serializable]
public abstract class BuildingBehavior : ScriptableObject
{
    public virtual bool CanBePlacedOnThis(Tile.TileType type)
    {
        return type switch
        {
            Tile.TileType.Grass or Tile.TileType.Sand => true,
            Tile.TileType.Water or Tile.TileType.Mountain => false,
            _ => throw new System.Exception("The tile types are fucked up.")
        };
    }
}