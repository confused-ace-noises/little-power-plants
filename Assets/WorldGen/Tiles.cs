using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting.InputSystem;
using UnityEngine;
using UnityEngine.Tilemaps;

public abstract class Tiles<T>
{
    public Vector2Int CurrentCenter { get; protected set; }
    public Dictionary<Vector2Int, T> LoadedTileDict {get; protected set;}
    public Dictionary<Vector2Int, T> Cache {get; protected set;}
    
    protected int halfLoadedHeight;
    protected int halfLoadedWidth;

    public Tiles(int halfLoadedWidth, int halfLoadedHeight)
    {
        this.halfLoadedWidth = halfLoadedWidth;
        this.halfLoadedHeight = halfLoadedHeight;
        this.LoadedTileDict = new();
        this.Cache = new();
        CurrentCenter = new(0,0);
    }
    
    public void MoveCenter(Vector2Int newPos)
    {
        Move(newPos-CurrentCenter);
    }

    protected virtual void ClearCacheReRender()
    {
        Cache.Clear();
        LoadedTileDict.Clear();

        for (int x = -halfLoadedWidth; x <= halfLoadedWidth; x++)
        for (int y = -halfLoadedHeight; y <= halfLoadedHeight; y++)
        {
            Vector2Int worldPos = new Vector2Int(
                CurrentCenter.x + x,
                CurrentCenter.y + y
            );

            var render = GetRenderable(worldPos);
            Cache.TryAdd(worldPos, render); 
            LoadFromCache(worldPos);
        }
    }

    protected virtual bool TryAddToLoaded(Vector2Int pos, T element)
    {
        return LoadedTileDict.TryAdd(pos, element);
    }

    protected abstract T GetRenderable(int x, int y);
    protected virtual T GetRenderable(Vector2Int pos)
    {
        return GetRenderable(pos.x, pos.y);
    }

    protected virtual void ReRenderWithCache()
    {
        LoadedTileDict.Clear();

        for (int x = -halfLoadedWidth; x <= halfLoadedWidth; x++)
        for (int y = -halfLoadedHeight; y <= halfLoadedHeight; y++)
        {
            Vector2Int worldPos = new Vector2Int(
                CurrentCenter.x + x,
                CurrentCenter.y + y
            );

            if (!LoadFromCache(worldPos))
            {
                TryAddToLoaded(worldPos, GetRenderable(worldPos));
            }
        }
    }

    protected bool UnloadToCache(Vector2Int pos)
    {
        bool exists = LoadedTileDict.Remove(pos, out T tile);

        if (exists)
        {
            Cache[pos] = tile;
        }

        return exists;
    }

    protected bool LoadFromCache(Vector2Int pos)
    {
        bool exists = Cache.ContainsKey(pos);
        if (exists)
        {
            TryAddToLoaded(pos, Cache[pos]);
        } 

        return exists;
    }

    protected bool OverwriteFromCache(Vector2Int pos)
    {
        bool exists = Cache.ContainsKey(pos);
        if (exists)
        {
            LoadedTileDict[pos] = Cache[pos];
        } else
        {
            LoadedTileDict.Remove(pos);
        }

        return exists;
    }

    /// <summary>
    ///     Muove nella direzione specificata Loaded
    /// </summary>
    /// <param name="xAxis">-1, 0 o 1</param>
    /// <param name="yAxis">-1, 0 o 1</param>
    protected void MoveOnce(int xAxis, int yAxis)
    {
        if (xAxis == 0 && yAxis == 0) return;
        
        MoveOnceX(xAxis);
        MoveOnceY(yAxis);
    }

    // amount can be -1, 0 or 1
    protected void MoveOnceX(int xAxis)
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
                TryAddToLoaded(positionAddX, GetRenderable(positionAddX.x, positionAddX.y));
            }
            UnloadToCache(positionRemX);
        }

        currentZero.x += xAxis;
        CurrentCenter = currentZero;
    }

    protected void MoveOnceY(int yAxis)
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
                TryAddToLoaded(positionAddY, GetRenderable(positionAddY.x, positionAddY.y));
            }
            UnloadToCache(positionRemY);
        }

        currentZero.y += yAxis;
        CurrentCenter = currentZero;
    }

    public T GetCachedOrCreateDontCache(Vector2Int vector)
    {
        bool isCached = Cache.ContainsKey(vector);
        if (isCached)
        {
            return Cache[vector];
        } else
        {
            return GetRenderable(vector.x, vector.y);
        }
    }

    public T GetCachedOrCreateAndCache(Vector2Int vector)
    {
        bool isCached = Cache.ContainsKey(vector);
        if (isCached)
        {
            return Cache[vector];
        } else
        {
            T toCache = GetRenderable(vector.x, vector.y);
            Cache.TryAdd(vector, toCache);
            return toCache;
        }
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

    public abstract Dictionary<Vector3Int, TileBase> IntoTilemapDict();

    public void ChangeLoadedArea(int newHalfLoadedWidth, int newHalfLoadedHeight)
    {
        this.halfLoadedWidth = newHalfLoadedWidth;
        this.halfLoadedHeight = newHalfLoadedHeight;

        ReRenderWithCache();
    }

    public void ForceTotalReRenderAndClearCacheForTheLoveOfGodDoNotCallMeNormallyImGoingToBreakSomethingIfYouDo()
    {
        ClearCacheReRender();
    }
}