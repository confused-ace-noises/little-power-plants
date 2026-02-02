using UnityEngine;
using UnityEngine.Tilemaps;

public class World : MonoBehaviour
{
    PerlinNoise perlinNoise;

    [Header("Tilemap")]
    public Tilemap terrainTilemap;

    [Header("Tiles")]
    public TileBase grassTile;
    public TileBase sandTile;
    public TileBase waterTile;

    [Header("Size rendered")]
    public int positiveX = 25;
    public int positiveY = 25;

    [Header("Thresholds")]
    public float sandThreshold = 0.4f;
    public float waterThreshold = 0.3f;


    [Header("Miscellaneous")]
    public int seed = 1234567;

    void Awake()
    {
        perlinNoise = new PerlinNoise(seed, 0.1f);
    }

    void Start()
    {
        Generate();
    }

    void Generate()
    {
        for (int x = -positiveX; x <= positiveX; x++)
        for (int y = -positiveY; y <= positiveY; y++)
        {
            var pos = new Vector3Int(x, y, 0);

            float noise = perlinNoise.GetHeight(x, y);

            TileBase tile =
                noise < waterThreshold ? waterTile :
                noise < sandThreshold ? sandTile :
                grassTile;

            terrainTilemap.SetTile(pos, tile);
        }
    }
}