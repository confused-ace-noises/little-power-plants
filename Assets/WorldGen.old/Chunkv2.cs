using UnityEngine;

public class Chunk
{
    public Vector2Int ChunkPos { get; private set; }
    public const int WIDTH = 16;
    public const int HEIGHT = 16;

    PerlinNoise perlinNoise;
    Thresholds thresholds;
    Tile[,] cached;

    public Chunk(Vector2Int position, PerlinNoise perlinNoise, Thresholds thresholds)
    {
        this.ChunkPos = position;
        this.perlinNoise = perlinNoise;
        this.thresholds = thresholds;
    }

    public Vector2Int Origin()
    {
        return ChunkPos * new Vector2Int(WIDTH, HEIGHT);
    }

    public Vector2Int LocalToWorld(Vector2Int local)
    {
        return Origin() + local;
    }

    public Tile[,] Render(bool recalculate = false)
    {
        if (cached != null && !recalculate)
            return cached;

        var result = new Tile[WIDTH, HEIGHT];

        var origin = Origin();

        for (int x = 0; x < WIDTH; x++)
        for (int y = 0; y < HEIGHT; y++)
        {
            float value = perlinNoise.GetValue(x + origin.x, y + origin.y);
            result[x,y] = thresholds.GetTile(value);
        }

        cached = result;

        return result;
    }
}