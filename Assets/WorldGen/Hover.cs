using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

// this should not have a cache btw
public class Overlay : Tiles<OverlayTile>
{
    public TileProvider tileProvider;
    public bool isDirty;

    public Overlay(int halfLoadedWidth, int halfLoadedHeight, TileProvider tileProvider) : base(halfLoadedWidth, halfLoadedHeight)
    {
        this.tileProvider = tileProvider;
    }

    public override Dictionary<Vector3Int, TileBase> IntoTilemapDict()
    {
        return LoadedTileDict.Where(kv => kv.Value.type != OverlayTile.TileType.Null).ToDictionary(
            kv => new Vector3Int(kv.Key.x, kv.Key.y, 0),
            kv => kv.Value.TileBase
        );
    }

    protected override OverlayTile GetRenderable(int x, int y)
    {
        if (Cache.TryGetValue(new(x,y), out OverlayTile tile))
        {
            return tile;
        } else
        {
            return new OverlayTile(OverlayTile.TileType.Null, tileProvider);
        }
    }

    public void Request(Vector2Int pos, OverlayTile.TileType type)
    {
        if (type == OverlayTile.TileType.Null)
        {
            Cache.Remove(pos);
        } else
        {
            Cache[pos] = new OverlayTile(type, tileProvider);
        }

        isDirty = true;

        OverwriteFromCache(pos);
    }
}