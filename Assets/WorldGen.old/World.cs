using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/*
public class World : MonoBehaviour
{
    [SerializeField] AtlasProvider atlasProvider;
    [SerializeField] int seed;    

    Dictionary<Vector2Int, Chunk> chunkCache;
    Dictionary<Vector2Int, Chunk> toRenderChunks;
    Dictionary<Vector2Int, GameObject> renderedChunks;
    PerlinNoise perlinNoise;


    void Awake()
    {
        perlinNoise = new PerlinNoise(seed);
        chunkCache = new Dictionary<Vector2Int, Chunk>();
        toRenderChunks = new Dictionary<Vector2Int, Chunk>();
    }

    void Start()
    {
        toRenderChunks.TryAdd(Vector2Int.zero, getChunk(Vector2Int.zero));        
    }

    void Update()
    {
        // toRenderChunks
        //     .AsEnumerable()
        //     .Select(keyValue => );
    }

    private Chunk getChunk(Vector2Int position)
    {
        return chunkCache.ContainsKey(position) 
            ? chunkCache[position] 
            : new Chunk(perlinNoise, position, atlasProvider);
    }
}
*/