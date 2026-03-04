using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu]
public class ZoneNotDiscovered : TileBase
{
    public TileProvider tileProvider;
    public WorldData data;

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        int posX = (Mathf.Abs(position.x) + data.halfLogicChunkWidth) % data.ChunkWidth;
        int posY = (Mathf.Abs(position.y) + data.halfLogicChunkHeight) % data.ChunkHeight;

        bool onEdgeX = posX == 0 || posX == data.ChunkWidth - 1;
        bool onEdgeY = posY == 0 || posY == data.ChunkHeight - 1;

        string tileName = (onEdgeX && onEdgeY)
            ? OverlayTile.GetNameOfType(OverlayTile.TileType.UndiscoveredWhite)
            : OverlayTile.GetNameOfType(OverlayTile.TileType.UndiscoveredBlack);

        TileBase resolved = tileProvider.GetTile(tileName);

        if (resolved != null)
        {
            // Copy the resolved tile's data into our tileData
            resolved.GetTileData(position, tilemap, ref tileData);
        }
    }
}