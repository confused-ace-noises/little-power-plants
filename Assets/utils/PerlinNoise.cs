using System;
using UnityEngine;

public class PerlinNoise
{
    private readonly int offsetX;
    private readonly int offsetY;
    // fake float, this is actually an int
    private readonly float scale;
    public readonly int seed;

    public PerlinNoise(int seed)
    {
        System.Random random = new System.Random(seed);
        offsetX = random.Next();
        offsetY = random.Next();
        scale = random.Next(1000, 10000) / 10000f; // fake float, this is actually an int
    }

    public float GetHeight(int x, int y)
    {
        // float thing = Mathf.Clamp(UnityEngine.Mathf.PerlinNoise((x + offsetX) * scale, (y + offsetY) * scale), 0, 1);
        float thing = Mathf.Clamp(UnityEngine.Mathf.PerlinNoise((x + offsetX), (y + offsetY)), 0, 1);
        Debug.Log("perlin noise at " + x + ", " + y + " is " + thing);
        return thing;
    }
}