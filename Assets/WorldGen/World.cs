using System.Collections.Generic;
using System.Linq;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    [Header("Tilemap")]
    public Tilemap terrainTilemap;
    public TileProvider tileProvider;

    [Header("Size rendered")]
    public int halfLoadedWidth = 25;
    public int halfLoadedHeight = 25;

    [Header("Thresholds")]
    public float mountain = 0.8f;
    public float grass = 0.5f;
    public float sand = 0.35f;

    [Header("Player")]
    public Transform playerPos;

    [Header("Miscellaneous")]
    public int seed = 1234567;

    PerlinNoise perlinNoise;
    Thresholds thresholds;
    
    Vector2Int playerTile;
    Vector2Int lastPlayerTile;
    
    Tiles tiles;
    HashSet<Vector3Int> previousRender;

    void Awake()
    {
        perlinNoise = new PerlinNoise(seed, 0.1f);
        thresholds = new Thresholds { tileProvider = tileProvider, grass = grass, mountain = mountain, sand = sand};
        tiles = new Tiles(halfLoadedWidth, halfLoadedWidth, perlinNoise, thresholds);
        playerPos.position = Vector3.zero;
    }

    void Start()
    {
        previousRender = tiles.IntoTilemapDict().Keys.ToHashSet();
        UpdatePlayerTile();
    }

    void Update()
    {
        UpdatePlayerTile();
    }

    void UpdatePlayerTile()
    {
        Vector3Int cell = terrainTilemap.WorldToCell(playerPos.position);
        playerTile = new Vector2Int(cell.x, cell.y);

        tiles.MoveCenter(playerTile);
        var dict = tiles.IntoTilemapDict();

        foreach (var old in previousRender)
        {
            if (!dict.ContainsKey(old))
                terrainTilemap.SetTile(old, null);
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

        terrainTilemap.SetTiles(positions, tilesArr);
        previousRender = positions.ToHashSet();
    }

    void UpdatePlayerTileForce()
    {
        
        Vector3Int cell = terrainTilemap.WorldToCell(playerPos.position);
        playerTile = new Vector2Int(cell.x, cell.y);

        tiles.MoveCenter(playerTile);
        var dict = tiles.IntoTilemapDict();

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
        previousRender = positions.ToHashSet();
    }
    
    public void ChangeRenderedArea(int newHalfRenderedWidth, int newHalfRenderedHeight)
    {
        this.halfLoadedWidth = newHalfRenderedWidth;
        this.halfLoadedHeight = newHalfRenderedHeight;

        tiles.ChangeLoadedArea(halfLoadedWidth, halfLoadedHeight);
    }

    [ContextMenu("DebugChangeRenderArea")]
    private void DebugChangeRenderArea()
    {
        tiles.ChangeLoadedArea(halfLoadedWidth, halfLoadedHeight);
    }

    [ContextMenu("ChangeSeed")]
    private void ChangeSeed()
    {
        perlinNoise = new PerlinNoise(seed, 0.1f);
        tiles.ForceTotalReRenderAndClearCacheForTheLoveOfGodDoNotCallMeNormallyImGoingToBreakSomethingIfYouDo();
        UpdatePlayerTileForce();
    }
}