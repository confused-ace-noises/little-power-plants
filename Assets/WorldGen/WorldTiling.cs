using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldTiling : Tiles<Tile>
{
    public int halfLogicChunkWidth;
    public int halfLogicChunkHeight;

    public int LogicChunkWidth() => halfLoadedWidth*2 +1;
    public int LogicChunkHeight() => halfLoadedHeight*2 +1;

    public HashSet<Vector2Int> loadedLogicChunks;


    // public MouseTrackSystem mouseTrackSystem;

    TileProvider tileProvider;
    WorldNoise perlinNoise;
    Thresholds thresholds;

    Overlay overlayTiles;

    public static event Action<Vector2Int> newChunkDiscovered;
    
    public WorldTiling(
        int halfLoadedWidth, 
        int halfLoadedHeight,
        int halfLogicChunkWidth,
        int halfLogicChunkHeight,
        WorldNoise perlinNoise, 
        Thresholds thresholds,
        TileProvider tileProvider,
        Overlay overlayTiles
       //  MouseTrackSystem mouseTrackSystem
    ) : base(halfLoadedWidth, halfLoadedHeight)
    {
        this.halfLogicChunkHeight = halfLogicChunkHeight;
        this.halfLogicChunkWidth = halfLogicChunkWidth;
        loadedLogicChunks = new();
        loadedLogicChunks.Add(new(0, 0));

        this.perlinNoise = perlinNoise;
        this.thresholds = thresholds;
        this.tileProvider = tileProvider;
        this.overlayTiles = overlayTiles;
       
       
        ClearCacheReRender();


        BuildingPlacementSystem.BuildingGotPlaced += BuildingWasPlaced;
        MouseTrackSystem.MouseClickInCell += HandleClick;
        MouseTrackSystem.MouseDragClickBetweenCells += HandleClick;
    }

    protected override void ClearCacheReRender()
    {
        loadedLogicChunks.Clear();
        loadedLogicChunks.Add(PosToLogicChunk(CurrentCenter));
        base.ClearCacheReRender();
    }

    const float MultBy = 0.3f;
    const float DivBy = 1.3f;
    protected override Tile GetRenderable(int x, int y) {
        return thresholds.GetTile(
            Mathf.SmoothStep(0f, 1f, (
                perlinNoise.GetValuePerlin(x, y) 
                + perlinNoise.GetValuePerlin(~x, ~y)
                * MultBy
            ) / DivBy)
            // perlinNoise.GetValuePerlin(x, y)
        ); 
    }
    
    public override Dictionary<Vector3Int, TileBase> IntoTilemapDict()
    {
        return LoadedTileDict.ToDictionary(
            kv => new Vector3Int(kv.Key.x, kv.Key.y, 0),
            kv => kv.Value.TileBase
        );
    }

    protected override bool TryAddToLoaded(Vector2Int pos, Tile element)
    {
        if (!loadedLogicChunks.Contains(PosToLogicChunk(pos)))
        {
            overlayTiles.Request(pos, OverlayTile.TileType.Undiscovered);
        }

        return LoadedTileDict.TryAdd(pos, element);
    }

    public Tile GetTile(Vector2Int vector)
    {
        return base.GetCachedOrCreateAndCache(vector);
    }

    public Vector2Int PosToLogicChunk(Vector2Int pos)
    {
        Func<int, int, int> conversion = (i, len) => Mathf.FloorToInt(((float)(i+len))/((float)((2*len) +1)));

        Vector2Int res = new Vector2Int(conversion(pos.x, halfLogicChunkWidth), conversion(pos.y, halfLogicChunkHeight));
        return res;
    }

    public Vector2Int[] LogicChunkToPos(Vector2Int pos)
    {
        Vector2Int center = LogicChunkCenterPoint(pos);

        Vector2Int lowerLeft = new (center.x - halfLoadedWidth, center.y - halfLoadedHeight);
        Vector2Int topRight = new (center.x + halfLoadedWidth, center.y + halfLoadedHeight);

        Vector2Int[] points = new Vector2Int[LogicChunkWidth() * LogicChunkHeight()];

        int i = 0;
        for (int x = lowerLeft.x; x <= topRight.x; x++)
        for (int y = lowerLeft.y; y <= topRight.y; y++)
        {
            points[i] = new Vector2Int(x, y);
            i++;
        }
        
        return points;
    }

    public Vector2Int LogicChunkCenterPoint(Vector2Int pos)
    {
        return new (pos.x*(2*halfLogicChunkWidth+1), pos.y*(2*halfLogicChunkHeight+1));
    }

    public bool AddNewChunk(Vector2Int pos)
    {
        bool DidntAlreadyExist = loadedLogicChunks.Add(pos);

        if (!DidntAlreadyExist)
        {
            return false;
        } else
        {
            foreach (Vector2Int piece in LogicChunkToPos(pos))
            {
                Cache[piece] = GetRenderable(piece);
                overlayTiles.Request(piece, OverlayTile.TileType.Null);
            }
            
            ReRenderWithCache();

            newChunkDiscovered?.Invoke(pos);
            return true;
        }
    }

    public bool CanYouPlaceHere(Vector2Int[] positions, Building building)
    {
        foreach (Vector2Int pos in positions)
        {
            if (!IsThisTileFineToPlaceOn(pos, building.buildingBehavior)) return false;
        }

        return true;
    }

    private bool IsThisTileFineToPlaceOn(Vector2Int pos, BuildingBehavior buildingBehavior)
    {
        Tile tile;
        bool exists = LoadedTileDict.TryGetValue(pos, out tile);

        if (!exists) return false;
        bool chunkExists = loadedLogicChunks.TryGetValue(PosToLogicChunk(pos), out Vector2Int _);

        return !tile.isOccupied && chunkExists && buildingBehavior.CanBePlacedOnThis(tile.type);
    }

    public void BuildingWasPlaced(Building building, Vector2Int[] positions)
    {
        foreach (Vector2Int pos in positions)
        {
            if (Cache.TryGetValue(pos, out Tile tileCached))
            {
                tileCached.SetOccupied(true);
            } else
            {
                Debug.LogError("A building was placed in undiscovered land. Will try to recover by creating it cached and setting it to occupied right away, expect bugs.");
                Tile tile = GetRenderable(pos);
                tile.SetOccupied(true);

                Cache.TryAdd(pos, tile);
            }

            if (LoadedTileDict.TryGetValue(pos, out Tile tileLoaded))
            {   
                tileLoaded.SetOccupied(true);
            }
        }
    }

    public void HandleClick(Vector2Int cell)
    {
        AddNewChunk(PosToLogicChunk(cell));
    }

    public void HandleClick(Vector2Int startDragCell, Vector2Int stopDragCell)
    {
        var startChunk = PosToLogicChunk(startDragCell);
        var stopChunk = PosToLogicChunk(stopDragCell);

        if (startChunk == stopChunk)
        {
            // we don't need to check if the selected chunk we're in
            // is valid or not, because AddNewChunk already does that for us
            AddNewChunk(startChunk);
        }
    }
}