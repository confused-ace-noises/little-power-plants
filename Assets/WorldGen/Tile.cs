using System;
using UnityEngine;

public struct Tile
{
    public const string TILE_ATLAS_NAME = "blocks";

    public enum TileType
    {
        Grass,
        Stone, 
        Water, 
        Mountain,
    }

    Sprite sprite;

    public TileType type;

    public Tile(TileType type, AtlasProvider atlasProvider)
    {
        this.type = type;
        this.sprite = atlasProvider[Tile.TILE_ATLAS_NAME, Tile.GetNameOfType(type), true];
    }

    public static string GetNameOfType(TileType type)
    {
        switch (type)
        {
            case TileType.Mountain:
            case TileType.Water:
            case TileType.Stone:
            case TileType.Grass:
                return "grass";

            default:
                throw new Exception("Questo punto non deve essere raggiungibile.");
        }
    }
}