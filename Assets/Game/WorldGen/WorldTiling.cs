using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
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
    public Dictionary<Vector2Int, int> chunkRisks;

    // public MouseTrackSystem mouseTrackSystem;

    TileProvider tileProvider;
    WorldNoise perlinNoise;
    Thresholds thresholds;

    Overlay overlayTiles;

    public static event Action<Vector2Int, int> newChunkDiscovered;
    
    const int FIRST_CHUNK_RISK = 1;

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
        loadedLogicChunks = new()
        {
            new(0, 0)
        };

        chunkRisks = new()
        {
            { new(0, 0), FIRST_CHUNK_RISK }
        };
        
        this.perlinNoise = perlinNoise;
        this.thresholds = thresholds;
        this.tileProvider = tileProvider;
        this.overlayTiles = overlayTiles;
       
       
        ClearCacheReRender();

        BuildingPlacementSystem.BuildingGotPlaced += BuildingWasPlaced;
        MouseTrackSystem.MouseClickInCell += HandleClick;
        MouseTrackSystem.MouseDragClickBetweenCells += HandleClick;
    }

    private int CalaculateRisksForNewChunks(Vector2Int chunkPos)
    {
        int risk = UnityEngine.Random.Range(0, 4); // [0;3]

        chunkRisks.Add(chunkPos, risk);
        return risk;
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
        // Debug.Log("chunck calculated: " + res);

        return res;
    }

    public (Vector2Int bottomLeft, Vector2Int topRight) CornersChunk(Vector2Int chunkPos)  {
        Vector2Int center = LogicChunkCenterPoint(chunkPos);

        return (new (center.x - halfLogicChunkWidth, center.y - halfLogicChunkHeight), new (center.x + halfLogicChunkWidth, center.y + halfLogicChunkHeight));
    }

    public Vector2Int[] LogicChunkToPos(Vector2Int pos)
    {
        var (bottomLeft, topRight) = CornersChunk(pos);

        // Debug.LogWarning($"got lowerLeft: {lowerLeft.x}; {lowerLeft.y}.\ngot topRight: {topRight.x}; {topRight.y}");

        Vector2Int[] points = new Vector2Int[LogicChunkWidth() * LogicChunkHeight()];

        int i = 0;
        for (int x = bottomLeft.x; x <= topRight.x; x++)
        for (int y = bottomLeft.y; y <= topRight.y; y++)
        {
            points[i] = new Vector2Int(x, y);
            i++;
        }
        
        return points;
    }

    public Vector2Int LogicChunkCenterPoint(Vector2Int pos)
    {
        // this is needed, you already checked, don't bother checking why this is right 
        // while you're tired again
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

            var risk = CalaculateRisksForNewChunks(pos);
            newChunkDiscovered?.Invoke(pos, risk);
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
        bool exists = LoadedTileDict.TryGetValue(pos, out Tile tile);

        if (!exists) return false;
        bool chunkExists = loadedLogicChunks.TryGetValue(PosToLogicChunk(pos), out Vector2Int _);

        bool caresOccupied = false;

        if (tile.IsOccupied())
        {
            caresOccupied = buildingBehavior.CaresIfOccupied(tile.Occupier().Value.Tuple);
        }

        return !caresOccupied && chunkExists && buildingBehavior.CanBePlacedOnThis(tile.type);
    }

    public void BuildingWasPlaced(Building building, GameObject placed, HashSet<Vector2Int> positions)
    {
        foreach (Vector2Int pos in positions)
        {
            var info = new BuildingOnTileInfo(building, placed, positions);
            
            if (Cache.TryGetValue(pos, out Tile tileCached))
            {
                tileCached.SetOccupiedTrue(info);
            } else
            {
                Debug.LogError("A building was placed in undiscovered land. Will try to recover by creating it cached and setting it to occupied right away, expect bugs.");
                Tile tile = GetRenderable(pos);
                tile.SetOccupiedTrue(info);

                Cache.TryAdd(pos, tile);
            }

            if (LoadedTileDict.TryGetValue(pos, out Tile tileLoaded))
            {   
                tileLoaded.SetOccupiedTrue(info);
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