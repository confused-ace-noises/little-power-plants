using System;
using UnityEngine.Tilemaps;

public class Tile
{
    public const string TILE_ATLAS_NAME = "TileAtlas";

    public enum TileType
    {
        Grass,
        Sand, 
        Water, 
        Mountain,
    }

    public TileBase TileBase { get; private set; }
    public TileType type;

    public Tile(TileType type, TileProvider tileProvider)
    {
        this.type = type;
        this.TileBase = tileProvider.GetTile(GetNameOfType(type)) ?? throw new Exception($"bad name of tile: {GetNameOfType(type)}");
    }

    public static string GetNameOfType(TileType type)
    {
        switch (type)
        {
            case TileType.Water: return "WaterTile";
            
            case TileType.Sand: return "SandTile";
            
            case TileType.Mountain:
            case TileType.Grass:
                return "GrassTile";

            default:
                throw new Exception("Questo punto non deve essere raggiungibile.");
        }
    }

    /*
    public static Tile DecideFromHeight(float height, AtlasProvider atlasProvider)
    {
        // polished algorithm goes here
        if (height >= 0.7)
        {
            return new Tile(TileType.Mountain, atlasProvider);
        } else if (height < 0.7)
        {
            return new Tile(TileType.Grass, atlasProvider);
        } else
        {
            throw new Exception("If you get this message, the worldGen algorithm is broken, please go fix it.");
        }
    }
    */
}