public class Thresholds
{
    public float mountain; // 1 to this
    public float grass; // mountain to this
    public float sand; // grass to this
    // water is lower than sand
    public TileProvider tileProvider;

    public Tile GetTile(float value)
    {
        return 
        value >= mountain ? new Tile(Tile.TileType.Mountain, tileProvider) :
            value >= grass ? new Tile(Tile.TileType.Grass, tileProvider) :
            value >= sand ? new Tile(Tile.TileType.Sand, tileProvider) :
            new Tile(Tile.TileType.Water, tileProvider);
    }
}