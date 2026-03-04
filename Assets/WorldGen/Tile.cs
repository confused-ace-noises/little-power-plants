using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tile
{
    // public const string TILE_ATLAS_NAME = "TileAtlas";

    

    public enum TileType
    {
        Grass,
        Sand, 
        Water, 
        Mountain,
    }

    public bool isOccupied;

    public TileBase TileBase { get; private set; }
    public TileType type;

    public Tile(TileType type, TileProvider tileProvider) 
    {
        isOccupied = false;
        this.type = type;
        this.TileBase = tileProvider.GetTile(GetNameOfType(type))!; //?? throw new Exception($"bad name of tile: {GetNameOfType(type)}");
    }

    public static string GetNameOfType(TileType type)
    {
        return type switch
        {
            TileType.Water => "WaterTile",
            TileType.Sand => "SandTile",
            TileType.Mountain => "StoneTile",
            TileType.Grass => "GrassTile",
            _ => throw new Exception("Questo punto non deve essere raggiungibile."),
        };
    }

    public void SetOccupied(bool setTo)
    {
        isOccupied = setTo;
    }

    /// <summary>
    /// toggles the 'isOccupied' property
    /// </summary>
    /// <returns>
    /// the new value of isOccupied
    /// </returns>
    public bool ToggleOccupied()
    {
        isOccupied = !isOccupied;
        return isOccupied;
    }
}

public class OverlayTile
{
    public enum TileType
    {
        HoverOk,
        HoverNo,
        UndiscoveredBlack,
        UndiscoveredWhite,
        Undiscovered,
        Null,
    }

    public TileBase TileBase { get; private set; }
    public TileType type;

    public OverlayTile(TileType type, TileProvider tileProvider) 
    {
        this.type = type;
        if (type != TileType.Null) // TileType.Null is never rendered
        {
            this.TileBase = tileProvider.GetTile(GetNameOfType(type)) ?? throw new Exception($"bad name of tile: {GetNameOfType(type)}");
        }
    }

    public static string GetNameOfType(TileType type)
    {
        return type switch
        {
            TileType.HoverOk => "HoverOk",
            TileType.HoverNo => "HoverNo",
            TileType.UndiscoveredBlack => "UndiscoveredBlack",
            TileType.UndiscoveredWhite => "UndiscoveredWhite",
            TileType.Undiscovered => "Undiscovered",
            _ => throw new Exception("Questo punto non deve essere raggiungibile."),
        };
    }
}