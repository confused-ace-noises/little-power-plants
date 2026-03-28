using Unity.VisualScripting;
using UnityEngine;

public class SinkPlacer : MonoBehaviour
{
    [SerializeField] World world;
    [SerializeField] Building building;
    [SerializeField] BuildingPlacementSystem buildingPlacementSystem;

    void Start()
    {
       WorldTiling.newChunkDiscovered += PlaceSink; 
    }

    void PlaceSink(Vector2Int chunkPos, int _)
    {
        if (chunkPos.x % 3 == 0 && chunkPos.y % 3 == 0)
        {
            var (bottomLeft, topRight) = world.worldTiles.CornersChunk(chunkPos);
            var center = world.worldTiles.LogicChunkCenterPoint(chunkPos);
            var centerPos = new Vector3Int(center.x, center.y, -3);

            if (world.worldTiles.CanYouPlaceHere(building.OccupiedCells(centerPos), building))
            {
                buildingPlacementSystem.ForcePlace(building, centerPos);
                goto HasCompleted;
            }

            for (int y = bottomLeft.y; y < topRight.y; y++)
            {
                for (int x = bottomLeft.x; x < topRight.x; x++)
                {
                    var pos = new Vector3Int(x, y, -3);
                    if (!world.worldTiles.CanYouPlaceHere(building.OccupiedCells(pos), building)) continue;
                    buildingPlacementSystem.ForcePlace(building, pos);
                    goto HasCompleted;
                }
            }

            HasCompleted:
            return;
        }
    }
}