using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tiles
{
    public Vector2Int CurrentCenter { get; private set; }
    public Dictionary<Vector2Int, Tile> LoadedTileDict {get; private set;}
    public Dictionary<Vector2Int, Tile> Cache {get; private set;}
    
    PerlinNoise perlinNoise;
    Thresholds thresholds;
    private int halfLoadedHeight;
    private int halfLoadedWidth;

    public Tiles(int halfLoadedWidth, int halfLoadedHeight, PerlinNoise perlinNoise, Thresholds thresholds)
    {
        this.halfLoadedWidth = halfLoadedWidth;
        this.halfLoadedHeight = halfLoadedHeight;
        this.perlinNoise = perlinNoise;
        this.thresholds = thresholds;
        this.LoadedTileDict = new();
        this.Cache = new();
        CurrentCenter = new(0,0);
        ClearCacheReRender();
    }

    public void MoveCenter(Vector2Int newPos)
    {
        Move(newPos-CurrentCenter);
    }

    private void ClearCacheReRender()
    {
        Cache.Clear();
        ReRender();
    }

    private void ReRender()
    {
        LoadedTileDict.Clear();

        for (int x = -halfLoadedWidth; x <= halfLoadedWidth; x++)
        for (int y = -halfLoadedHeight; y <= halfLoadedHeight; y++)
        {
            Vector2Int worldPos = new Vector2Int(
                CurrentCenter.x + x,
                CurrentCenter.y + y
            );

            LoadedTileDict.TryAdd(worldPos, thresholds.GetTile(perlinNoise.GetValue(x, y)));
        }
    }

    bool UnloadToCache(Vector2Int pos)
    {
        bool exists = LoadedTileDict.Remove(pos, out Tile tile);

        if (exists)
        {
            Cache[pos] = tile;
        }

        return exists;
    }

    bool LoadFromCache(Vector2Int pos)
    {
        bool exists = Cache.ContainsKey(pos);
        if (exists)
        {
            LoadedTileDict[pos] = Cache[pos];
        } 

        return exists;
    }

    /// <summary>
    ///     Muove nella direzione specificata Loaded
    /// </summary>
    /// <param name="xAxis">-1, 0 o 1</param>
    /// <param name="yAxis">-1, 0 o 1</param>
    private void MoveOnce(int xAxis, int yAxis)
    {
        if (xAxis == 0 && yAxis == 0) return;
        
        MoveOnceX(xAxis);
        MoveOnceY(yAxis);
    }

    // amount can be -1, 0 or 1
    private void MoveOnceX(int xAxis)
    {
        if (xAxis == 0) return;

        Vector2Int positionAddX;
        Vector2Int positionRemX;
        Vector2Int currentZero = CurrentCenter;

        int leftEdge = currentZero.x - halfLoadedWidth;
        int rightEdge = currentZero.x + halfLoadedWidth;

        switch (xAxis)
        {
            case 0: return;
            case 1:
                positionAddX = new(rightEdge + 1, 0);
                positionRemX = new(leftEdge, 0); // -1 old
                break;
            case -1:
                positionAddX = new(leftEdge - 1, 0);
                positionRemX = new(rightEdge, 0); // +1 old
                break;
            default:
                throw new System.Exception("xAxis deve essere 1, 0 o -1");
        }

        for (int y = -halfLoadedHeight + CurrentCenter.y; y <= halfLoadedHeight + CurrentCenter.y; y++)
        {
            positionAddX.y = y;
            positionRemX.y = y;

            bool wasCached = LoadFromCache(positionAddX);
            if (!wasCached)
            {
                LoadedTileDict[positionAddX] = 
                    thresholds.GetTile(perlinNoise.GetValue(positionAddX.x, positionAddX.y));
            }
            UnloadToCache(positionRemX);
        }

        currentZero.x += xAxis;
        CurrentCenter = currentZero;
    }

    private void MoveOnceY(int yAxis)
    {
        if (yAxis == 0) return;

        Vector2Int positionAddY;
        Vector2Int positionRemY;
        Vector2Int currentZero = CurrentCenter;
        
        int bottomEdge = currentZero.y - halfLoadedHeight;
        int topEdge = currentZero.y + halfLoadedHeight;

        switch (yAxis)
        {
            case 0: return;
            case 1:
                positionAddY = new(0, topEdge + 1);
                positionRemY = new(0, bottomEdge);
                break;
            case -1:
                positionAddY = new(0, bottomEdge - 1);
                positionRemY = new(0, topEdge);
                break;
            default:
                throw new System.Exception("yAxis deve essere 1, 0 o -1");
        }

        for (int x = -halfLoadedWidth + CurrentCenter.x; x <= halfLoadedWidth + CurrentCenter.x; x++)
        {
            positionAddY.x = x;
            positionRemY.x = x;

            bool wasCached = LoadFromCache(positionAddY);
            if (!wasCached)
            {
                LoadedTileDict[positionAddY] = 
                    thresholds.GetTile(perlinNoise.GetValue(positionAddY.x, positionAddY.y));
            }
            UnloadToCache(positionRemY);
        }

        currentZero.y += yAxis;
        CurrentCenter = currentZero;
    }

    public void Move(Vector2Int towards)
    {
        if (towards.x < 0)
        {
            for (int x = 0; x > towards.x; x--)
            {
                MoveOnceX(-1);
            }
        } else 
        {
            for (int x = 0; x < towards.x; x++)
            {
                MoveOnceX(+1);
            }
        }

        if (towards.y < 0)
        {
            for (int y = 0; y > towards.y; y--)
            {
                MoveOnceY(-1);
            }
        } else 
        {
            for (int y = 0; y < towards.y; y++)
            {
                MoveOnceY(+1);
            }
        }
    }

    public Dictionary<Vector3Int, TileBase> IntoTilemapDict()
    {
        return LoadedTileDict.ToDictionary(
            kv => new Vector3Int(kv.Key.x, kv.Key.y, 0),
            kv => kv.Value.TileBase
        );
    }

    public void ChangeLoadedArea(int newHalfLoadedWidth, int newHalfLoadedHeight)
    {
        this.halfLoadedWidth = newHalfLoadedWidth;
        this.halfLoadedHeight = newHalfLoadedHeight;

        ReRender();
    }

    public void ForceTotalReRenderAndClearCacheForTheLoveOfGodDoNotCallMeNormallyImGoingToBreakSomethingIfYouDo()
    {
        ClearCacheReRender();
    }
}