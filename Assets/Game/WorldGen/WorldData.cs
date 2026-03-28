using UnityEngine;

[CreateAssetMenu]
public class WorldData : ScriptableObject
{
    [Header("Size rendered")]
    public int halfLoadedWidth = 25;
    public int halfLoadedHeight = 25;

    [Header("Chunks")]
    public int halfLogicChunkWidth;
    public int halfLogicChunkHeight;

    [Header("Thresholds")]
    public float mountain = 0.8f;
    public float grass = 0.5f;
    public float sand = 0.35f;

    [Header("WorldGen")]
    public int seed;

    public int ChunkWidth {
        get => (halfLogicChunkWidth * 2) + 1;
        set => ChunkWidth = (value - 1) / 2;
    }

    public int ChunkHeight {
        get => (halfLogicChunkHeight * 2) + 1;
        set => ChunkHeight = (value - 1) / 2;
    }
}