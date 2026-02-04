using UnityEngine;

public class PerlinNoise
{
    private readonly float offsetX;
    private readonly float offsetY;
    private readonly float scale;
    public readonly int seed;

    public PerlinNoise(int seed, float scale = 0.05f)
    {
        this.seed = seed;
        this.scale = scale;

        System.Random random = new System.Random(seed);

        offsetX = random.Next(-100_000, 100_000);
        offsetY = random.Next(-100_000, 100_000);
    }

    public float GetValue(int x, int y)
    {
        float sampleX = (x + offsetX) * scale;
        float sampleY = (y + offsetY) * scale;

        float value = Mathf.Clamp01(Mathf.PerlinNoise(sampleX, sampleY));

        return value;
    }
}