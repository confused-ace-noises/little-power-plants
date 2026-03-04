using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Numerics;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileIsBeingSelected : MonoBehaviour
{
    const string OkIlluminatedTile = "HoverOk";
    const string ErrIlluminatedTile = "HoverNo";

    // public const float PERFECT_MOUSE_OFFSET = 13f;

    public Tilemap tilemap;
    public World world;
    public TileProvider illuminatedTiles;

    bool DontPlaceOverride;

    public void Start()
    {
        MouseTrackSystem.MouseMovedIntoCell += ApplyOverlay;
        MouseTrackSystem.MouseMovedOutOfCell += RemoveOverlay;
        MouseTrackSystem.MouseDragHasntClickedYet += ApplyOverlayToMultiple;
        MouseTrackSystem.MouseDragClickBetweenCells += RemoveOverlayMultiple;
        DontPlaceOverride = false;
        isDoingDrag = false;
        // MouseTrackSystem.NewMouseStateInCell += UpdateOverlyOnCLick;
    }

    // public void UpdateOverlyOnCLick(Vector3Int pos, bool state) => ApplyOverlay(pos);
    public void ApplyOverlay(Vector3Int pos)
    {
        if (!DontPlaceOverride && !isDoingDrag)
        {
            tilemap.SetTile(pos, DecideColorTile((Vector2Int)pos));
        }
    }

    private bool isDoingDrag;
    private HashSet<Vector2Int> oldDraggedCells = new();
    public void ApplyOverlayToMultiple(Vector2Int startPos, Vector2Int nowPos)
    {
        if (DontPlaceOverride) return;
     
        isDoingDrag = true;

        (Vector2Int start, Vector2Int end) = MouseTrackSystem.BuildSquare(startPos, nowPos);

        HashSet<Vector2Int> newDraggedCells = new();

        for(int x = start.x; x <= end.x; x++)
        for(int y = start.y; y <= end.y; y++)
        {
            Vector2Int cell = new(x, y);

            if (!oldDraggedCells.Remove(cell))
            {
                tilemap.SetTile(
                    new Vector3Int(x, y, 0),
                    DecideColorTile(cell)
                );
            }

            newDraggedCells.Add(cell);
        }

        foreach (Vector2Int oldCell in oldDraggedCells)
        {
            tilemap.SetTile(new(oldCell.x, oldCell.y, 0), null);
        }
        
        oldDraggedCells = newDraggedCells;
    }

    public void RemoveOverlayMultiple(Vector2Int startPos, Vector2Int nowPos)
    {
        ApplyOverlayToMultiple(startPos, nowPos);

        foreach (Vector2Int cell in oldDraggedCells)
        {
            tilemap.SetTile(new(cell.x, cell.y, 0), null);
        }

        oldDraggedCells.Clear();
        isDoingDrag = false;
    }

    public void RemoveOverlay(Vector3Int oldPos)
    {
        if (!DontPlaceOverride && !isDoingDrag)
        {
            tilemap.SetTile(oldPos, null);
        }
    }

    TileBase DecideColorTile(Vector2Int position)
    {
        if (!world.worldTiles.loadedLogicChunks.Contains(world.worldTiles.PosToLogicChunk(position)))
        {
            return illuminatedTiles.GetTile(ErrIlluminatedTile)!;
        } else
        {
            #nullable enable
            TileBase? tileBase = illuminatedTiles.GetTile(world.worldTiles.GetTile(position).type switch
            {
                Tile.TileType.Grass or Tile.TileType.Sand => OkIlluminatedTile,
                Tile.TileType.Water or Tile.TileType.Mountain => ErrIlluminatedTile,
                _ => throw new System.Exception("Non tutti i casi sono coperti in TileIsBeingSelected")
            })!; // so che esistiono quindi posso ignorare null
                        
            return tileBase;
        }

    }

    public void SetPlaceOverride(bool placeOverride)
    {
        DontPlaceOverride = placeOverride;
    }

    public void TogglePlaceOverride()
    {
        DontPlaceOverride = !DontPlaceOverride;
    }
}