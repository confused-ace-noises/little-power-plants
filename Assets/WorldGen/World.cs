using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    [Header("TerrainTilemap")]
    public Tilemap terrainTilemap;
    public TileProvider terrainTileProvider;

    [Header("OveralyTilemap")]
    public Tilemap overlayTilemap;
    public TileProvider overlayTileProvider;

    [Header("Data")]
    public WorldData data;

    [Header("Player")]
    public Transform playerPos;

    [Header("Buildings")]
    public Building[] buildings;

    [Header("Miscellaneous")]
    public int seed = 1234567;
    public MouseTrackSystem mouseTrackSystem;

    public WorldNoise perlinNoise;
    public Thresholds thresholds;
    
    [NonSerialized] public Vector2Int playerTile;
    [NonSerialized] public Vector2Int lastPlayerTile;
    
    
    [NonSerialized] public Overlay overlayTiles;
    [NonSerialized] public HashSet<Vector3Int> previousRenderOverlay;
    
    [NonSerialized] public WorldTiling worldTiles;
    [NonSerialized] public HashSet<Vector3Int> previousRenderWorld;

    [NonSerialized] bool worldIsDirty;

    UnityEvent<Vector2Int[]> buildingIsPlaced;

    void Awake()
    {
        perlinNoise = new WorldNoise(seed, 0.07f);
        thresholds = new Thresholds(data.sand, data.grass, data.mountain, terrainTileProvider);
        playerPos.position = Vector3.zero;
        overlayTiles = new Overlay(data.halfLoadedWidth, data.halfLoadedWidth, overlayTileProvider);
        worldTiles = new WorldTiling(data.halfLoadedWidth, data.halfLoadedWidth, data.halfLogicChunkWidth, data.halfLogicChunkWidth, perlinNoise, thresholds, terrainTileProvider, overlayTiles);
    }

    void Start()
    {
        lastPlayerTile = new (0, 0);
        playerTile = new (0, 0);
        
        worldIsDirty = true;
        overlayTiles.isDirty = true;

        previousRenderWorld = worldTiles.IntoTilemapDict().Keys.ToHashSet();
        UpdateTiles(worldTiles, terrainTilemap, ref previousRenderWorld);

        previousRenderOverlay = overlayTiles.IntoTilemapDict().Keys.ToHashSet();
        UpdateTiles(overlayTiles, overlayTilemap, ref previousRenderOverlay);
        
        overlayTiles.isDirty = false;
        worldIsDirty = false;
    }

    void Update()
    {
        playerTile = (Vector2Int) terrainTilemap.WorldToCell(playerPos.position);
        worldIsDirty = playerTile != lastPlayerTile;
        
        if (worldIsDirty)
            UpdateTiles(worldTiles, terrainTilemap, ref previousRenderWorld);
        
        if (overlayTiles.isDirty || worldIsDirty)
            UpdateTiles(overlayTiles, overlayTilemap, ref previousRenderOverlay);
        
        lastPlayerTile = playerTile;
    }

    void UpdateTiles<T>(Tiles<T> tiles, Tilemap tilemap, ref HashSet<Vector3Int> oldRender)
    {
        tiles.MoveCenter(playerTile);
        var dict = tiles.IntoTilemapDict();

        foreach (var old in oldRender)
        {
            if (!dict.ContainsKey(old))
                tilemap.SetTile(old, null);
        }

        var positions = new Vector3Int[dict.Count];
        var tilesArr  = new TileBase[dict.Count];

        int i = 0;
        foreach (var kv in dict)
        {
            positions[i] = kv.Key;
            tilesArr[i]  = kv.Value;
            i++;
        }

        tilemap.SetTiles(positions, tilesArr);
        oldRender = positions.ToHashSet();
    }

    void UpdatePlayerTileForce()
    {
        
        Vector3Int cell = terrainTilemap.WorldToCell(playerPos.position);
        playerTile = new Vector2Int(cell.x, cell.y);

        worldTiles.MoveCenter(playerTile);
        var dict = worldTiles.IntoTilemapDict();

        var positions = new Vector3Int[dict.Count];
        var tilesArr  = new TileBase[dict.Count];

        int i = 0;
        foreach (var kv in dict)
        {
            positions[i] = kv.Key;
            tilesArr[i]  = kv.Value;
            i++;
        }

        terrainTilemap.SetTiles(positions, null);
        terrainTilemap.SetTiles(positions, tilesArr);
        previousRenderWorld = positions.ToHashSet();
    }
    
    public void ChangeRenderedArea(int newHalfRenderedWidth, int newHalfRenderedHeight)
    {
        data.halfLoadedWidth = newHalfRenderedWidth;
        data.halfLoadedHeight = newHalfRenderedHeight;

        worldTiles.ChangeLoadedArea(data.halfLoadedWidth, data.halfLoadedHeight);
    }

    [ContextMenu("DebugChangeRenderArea")]
    private void DebugChangeRenderArea()
    {
        worldTiles.ChangeLoadedArea(data.halfLoadedWidth, data.halfLoadedHeight);
    }

    [ContextMenu("ChangeSeed")]
    private void ChangeSeed()
    {
        // perlinNoise = new WorldNoise(seed, 0.1f);
        perlinNoise = new WorldNoise(seed, 0.07f);
        thresholds = new Thresholds(data.sand, data.grass, data.mountain, terrainTileProvider);
        worldTiles = new WorldTiling(data.halfLoadedWidth, data.halfLoadedWidth, data.halfLogicChunkWidth, data.halfLogicChunkWidth, perlinNoise, thresholds, terrainTileProvider, overlayTiles);
        worldTiles.ForceTotalReRenderAndClearCacheForTheLoveOfGodDoNotCallMeNormallyImGoingToBreakSomethingIfYouDo();
        UpdatePlayerTileForce();
    }
}