using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class EvilPlantPlacer : MonoBehaviour
{
    public Building building;
    public World world;
    public BuildingPlacementSystem buildingPlacementSystem;
    public int maxRange;

    // public PendingBuilding? lastPending;
    #nullable enable
    PendingBuilding? lastPendingBuilding;

    Vector2Int[] occupiedCellsBuffer;

    void Start()
    {
        occupiedCellsBuffer = new Vector2Int[building.cells.x * building.cells.y];
        WorldTiling.newChunkDiscovered += PlacePlants;
    }

    private IEnumerator PlacePlantsWithCheck(Vector2Int chunkPos, int risk)
    {
        yield return new WaitUntil(() => lastPendingBuilding == null || lastPendingBuilding.isDone); // wait until canGoOn is true
        lastPendingBuilding = null;

        var (bottomLeft, topRight) = world.worldTiles.CornersChunk(chunkPos);
        int area = (topRight.x - bottomLeft.x) * (topRight.y - bottomLeft.y);
        List<Vector3Int> positions = new List<Vector3Int>(area / maxRange); // rough capacity estimate

        for (int y = bottomLeft.y; y < topRight.y; y++)
            for (int x = bottomLeft.x; x < topRight.x; x++)
            {
                var pos = new Vector3Int(x, y, -3);
                FillOccupiedCells(pos);
                if (!world.worldTiles.CanYouPlaceHere(occupiedCellsBuffer, building)) continue;
                if (UnityEngine.Random.Range(0, maxRange - risk + 1) == 0) // +1 to make it inclusive
                {
                    positions.Add(pos);
                    x++; // advance 1 in x so that you never get overlapping buildings
                }
            }

        if (positions.Count != 0)
            lastPendingBuilding = buildingPlacementSystem.ForcePlaceMany(building, positions.ToArray());
    }

    void PlacePlants(Vector2Int chunkPos, int risk)
    {
        StartCoroutine(PlacePlantsWithCheck(chunkPos, risk));
    }

    void FillOccupiedCells(Vector3Int pos)
    {
        int index = 0;
        for (int x = 0; x < building.cells.x; x++)
            for (int y = 0; y < building.cells.y; y++)
                occupiedCellsBuffer[index++] = new Vector2Int(pos.x + x, pos.y + y);
    }
}

public class Flat2dIndexer<T>
{
    T[] array;
    int nColumns;

    public Flat2dIndexer(int rows, int columns) 
    {
        array = new T[rows * columns];
        nColumns = columns;
    } 

    public Flat2dIndexer(T[] starting, int columns) 
    {
        array = starting;
        nColumns = columns;
    } 

    public T this[int row, int col]
    {
        get => array[row * nColumns + col];
        set => array[row * nColumns + col] = value;
    }
}