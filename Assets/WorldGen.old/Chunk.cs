using UnityEngine;

public class ChunkData
{
    public const int CHUNK_WIDTH = 16;
    public const int CHUNK_HEIGHT = 16;

    public Tile[,] tileArray {get; private set;}
    public Vector2Int position {get; private set;}
    AtlasProvider atlasProvider;
    PerlinNoise perlinNoise;

    public ChunkData(PerlinNoise perlinNoise, Vector2Int position, AtlasProvider atlasProvider)
    {
        this.atlasProvider = atlasProvider;
        this.position = position;
        this.perlinNoise = perlinNoise;

        // si può chiamare qui perchè positon, atlasProvider e perlinNoise sono
        // stati assegnati
        CreateTiles(position);
    }

    // NON chiamare questa funzione senza guardarla bene, muta il tileArray
    private void CreateTiles(Vector2Int position)
    {
        Tile[,] result = new Tile[CHUNK_WIDTH, CHUNK_HEIGHT];

        Vector2Int worldPos = position * new Vector2Int(CHUNK_WIDTH, CHUNK_HEIGHT);

        //  column-first per cercare di ridurre al minimo le cache misses
        for (int x = worldPos.x; x < worldPos.x + CHUNK_WIDTH; x++)
        for (int y = worldPos.y; y < worldPos.y + CHUNK_HEIGHT; y++)
        {
            float height = this.perlinNoise.GetHeight(x, y);

            result[x-worldPos.x, y-worldPos.y] = Tile.DecideFromHeight(height, atlasProvider);
        }

        this.tileArray = result;
    }
}

public class Chunk
{
    ChunkData data;
    GameObject gameObject;
    MeshFilter meshFilter;
}