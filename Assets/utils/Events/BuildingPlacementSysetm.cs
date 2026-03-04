using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class BuildingPlacementSystem : MonoBehaviour
{
    public static event Action<Building, Vector2Int[]> BuildingGotPlaced;

    [Header("WorldInfo")]
    [SerializeField] Grid grid;
    [SerializeField] World world;
    [SerializeField] private TileIsBeingSelected tileIsBeingSelected;
    
    [Header("Materials")]
    [SerializeField] private Material translucentMaterialOk;
    [SerializeField] private Material translucentMaterialNo;

    [Header("Debug")]
    [SerializeField] private Building debugBuilding;
    
    
    private Building currentBuilding;
    private GameObject previewGO;

    private Vector3Int lastHoveredCell;

    public void Awake()
    {
        MouseTrackSystem.MouseMovedIntoCell += OnMouseMovedIntoCell;
        MouseTrackSystem.MouseMovedOutOfCell += OnMouseMovedOutOfCell;
        MouseTrackSystem.NewMouseStateInCell += TryPlaceBuilding;
    } 

    // Call this to start placing a building
    
    public void BuildingIsGettingPlaced(Building building)
    {
        tileIsBeingSelected.SetPlaceOverride(true);

        currentBuilding = building;

        // Instantiate a preview object
        previewGO = GameObject.Instantiate(building.prefab);
        SetPreviewMaterial(previewGO, translucentMaterialOk);
    }

    [ContextMenu("DebugPlaceBuilding")]
    private void DebugPlaceBuilding()
    {
        BuildingIsGettingPlaced(debugBuilding);
    }

    private void OnMouseMovedIntoCell(Vector3Int cell)
    {
        if (currentBuilding == null) return;

        // Snap preview to bottom-left of the building
        Vector3 snapped = grid.CellToWorld(cell);
        Vector3 offset = new Vector3(
            currentBuilding.cells.x / 2f,
            currentBuilding.cells.y / 2f,
            -3 // z offset to make the building actually show up
        );
        previewGO.transform.position = snapped + offset;

        SetPreviewMaterial(previewGO, world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), currentBuilding) ? translucentMaterialOk : translucentMaterialNo);

        lastHoveredCell = cell;
    }

    private void OnMouseMovedOutOfCell(Vector3Int cell)
    {
        // Optional: handle highlighting tiles
    }

    // Call this when the player clicks to place
    public void TryPlaceBuilding(Vector3Int cell, bool MouseState)
    {
        if (currentBuilding == null || !MouseState) return;

        if (!world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), currentBuilding)) {
            StartCoroutine(BlinkRoutine(previewGO, 0.1f));
            return;
        }


        Vector2Int pos = (Vector2Int) cell;

        // Calculate all occupied cells
        Vector2Int[] occupiedCells = OccupiedCells(cell);

        // Optionally: check if all cells are free (you can hook into your GridManager here)

        // Place the actual building prefab
        GameObject placedGO = GameObject.Instantiate(currentBuilding.prefab);
        Vector3 snapped = grid.CellToWorld(cell);
        Vector3 offset = new Vector3(
            Mathf.Floor(currentBuilding.cells.x / 2f),
            Mathf.Floor(currentBuilding.cells.y / 2f),
            -3 // z offset to make the building actually show up
        );
        placedGO.transform.position = snapped + offset;

        SetPreviewMaterial(placedGO, currentBuilding.standardMaterial);

        // Fire event
        BuildingGotPlaced?.Invoke(currentBuilding, occupiedCells);

        // Destroy preview and reset
        GameObject.Destroy(previewGO);
        tileIsBeingSelected.SetPlaceOverride(false);
        previewGO = null;
        currentBuilding = null;
    }

    IEnumerator BlinkRoutine(GameObject GO, float delaySecs)
    {
        for (int i = 0; i < 3; i++) // repeat indefinitely
        {
            SetPreviewMaterial(GO, translucentMaterialOk);  
            yield return new WaitForSeconds(delaySecs);
            SetPreviewMaterial(GO, translucentMaterialNo);  
            yield return new WaitForSeconds(delaySecs);
        }
        SetPreviewMaterial(GO, translucentMaterialOk);  
    }

    private static void SetPreviewMaterial(GameObject go, Material mat)
    {
        var renderer = go.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            renderer.material = mat;
        }
    }

    private Vector2Int[] OccupiedCells (Vector3Int pos)
    {
        Vector2Int[] occupiedCells = new Vector2Int[currentBuilding.cells.x * currentBuilding.cells.y];
        int index = 0;
        for (int x = 0; x < currentBuilding.cells.x; x++)
        {
            for (int y = 0; y < currentBuilding.cells.y; y++)
            {
                occupiedCells[index++] = new Vector2Int(pos.x + x, pos.y + y);
            }
        }

        return occupiedCells;
    }
}