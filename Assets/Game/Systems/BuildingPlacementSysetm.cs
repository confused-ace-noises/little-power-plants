using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class BuildingPlacementSystem : MonoBehaviour
{
    public static event Action<Building, GameObject, Vector2Int[]> BuildingGotPlaced;

    public SpawnNewTypeButtons spawnNewTypeButtons;

    [Header("WorldInfo")]
    [SerializeField] Grid grid;
    [SerializeField] World world;
    [SerializeField] private TileIsBeingSelected tileIsBeingSelected;
    
    [Header("Materials")]
    [SerializeField] private Material translucentMaterialOk;
    [SerializeField] private Material translucentMaterialNo;

    [Header("Debug")]
    [SerializeField] private Building debugBuilding;
    
    
    #nullable enable
    private BuildingPlacer? currentPlacer;

    public void Awake()
    {
        MouseTrackSystem.MouseMovedIntoCell += OnMouseMovedIntoCell;
        // MouseTrackSystem.MouseMovedOutOfCell += OnMouseMovedOutOfCell;
        MouseTrackSystem.NewMouseStateInCell += TryPlaceBuilding;
        spawnNewTypeButtons.GO_GotPlaced += SubscribeToNewButton;
    } 

    public void SubscribeToNewButton(GameObject GO)
    {
        if (currentPlacer != null)
        {
            currentPlacer.OnCancel();
            currentPlacer = null;
        }

        GO.GetComponent<MakeButtonEventEscapePrefab>().clicked += StartPlacingBuilding;
    }

    [ContextMenu("DebugPlaceBuilding")]
    private void DebugPlaceBuilding()
    {
        StartPlacingBuilding(debugBuilding);
    }

    // Call this to start placing a building
    public void StartPlacingBuilding(Building building)
    {
        tileIsBeingSelected.SetOverrideTrue();

        currentPlacer = building.buildingBehavior.GetPlacer();
        currentPlacer.BuildingGotPlaced += PropagateBuildingPlaced;

        currentPlacer.Initialize(building, grid, world, translucentMaterialOk, translucentMaterialNo, this);
        currentPlacer.SetUpRun(this);
        currentPlacer.OnBegin();
    }

    private void OnMouseMovedIntoCell(Vector3Int cell)
    {
        if (currentPlacer == null) return;

        // Snap preview to bottom-left of the building
        currentPlacer.OnHover(cell);
    }

    /*
    private void OnMouseMovedOutOfCell(Vector3Int cell)
    {
        // Optional: handle highlighting tiles
    }
    */

    // Call this when the player clicks to place
    
    public void TryPlaceBuilding(Vector3Int cell, bool MouseState)
    {
        if (currentPlacer == null || !MouseState) return;

        bool placed = currentPlacer.OnConfirm(cell);
        if (placed)
        {
            tileIsBeingSelected.SetOverrideFalse();
            currentPlacer.BuildingGotPlaced -= PropagateBuildingPlaced;
            currentPlacer = null;
        }
    }

    public static void PropagateBuildingPlaced(Building building, GameObject GO, Vector2Int[] pos)
    {
        BuildingGotPlaced?.Invoke(building, GO, pos);
    }
}

public class BuildingPlacer
{
    protected MonoBehaviour runner;

    public Building building {get; protected set;}
    protected Grid grid;
    protected World world;

    protected Material OkMaterial;
    protected Material NoMaterial;
    private GameObject preview;

    public event Action<Building, GameObject, Vector2Int[]> BuildingGotPlaced;

    protected void InvokeBuiildingGotPlaced(Building building, GameObject GO, Vector2Int[] pos)
    {
        BuildingGotPlaced?.Invoke(building, GO, pos);
    }

    public virtual void Initialize(Building building, Grid grid, World world, Material OkMaterial, Material NoMaterial, MonoBehaviour runner)
    {
        this.building = building;
        this.grid = grid;
        this.world = world;
        this.OkMaterial = OkMaterial;
        this.NoMaterial = NoMaterial;
    }

    public virtual void SetUpRun(MonoBehaviour runner)
    {
        this.runner = runner;
    }

    // Called once when placement mode starts
    public virtual void OnBegin()
    {
        preview = GameObject.Instantiate(building.prefab);
        SetPreviewMaterial(preview, OkMaterial);
        preview.SetActive(false);
    }

    // Called every time the mouse moves to a new cell
    public virtual void OnHover(Vector3Int cell)
    {
        preview.SetActive(true);

        preview.transform.position = GetPlacement(cell);

        SetPreviewMaterialSmart(preview, cell);
    }

    // Called when the player clicks
    // Returns a GO if placement is complete (and system should exit placement mode)
    // returns null otherwise
    public virtual bool OnConfirm(Vector3Int cell)
    {
        if (!world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), building)) {
            Debug.Log($"is runner null: {runner == null}");
            runner.StartCoroutine(BlinkRoutine(preview, 0.1f));
            SetPreviewMaterialSmart(preview, cell);
            return false;
        }

        // Optionally: check if all cells are free (you can hook into your GridManager here)

        // Place the actual building prefab
        // GameObject placedGO = GameObject.Instantiate(building.prefab);
        preview.transform.position = GetPlacement(cell);

        SetPreviewMaterial(preview, building.standardMaterial);

        BuildingGotPlaced?.Invoke(building, preview, OccupiedCells(cell));
        preview = null;
        runner = null;
        return true;
    }

    // Called when placement is cancelled (ESC, etc.)
    public virtual void OnCancel()
    {
        preview = null;
        runner = null;
    }

    protected IEnumerator BlinkRoutine(GameObject GO, float delaySecs)
    {
        for (int i = 0; i < 3; i++)
        {
            SetPreviewMaterial(GO, OkMaterial);  
            yield return new WaitForSeconds(delaySecs);
            SetPreviewMaterial(GO, NoMaterial);  
            yield return new WaitForSeconds(delaySecs);
        }
        // SetPreviewMaterial(GO, OkMaterial);  
    }

    protected static void SetPreviewMaterial(GameObject go, Material mat)
    {
        if (go.TryGetComponent<SpriteRenderer>(out var renderer))
        {
            renderer.material = mat;
        }
    }

    protected void SetPreviewMaterialSmart(GameObject go, Vector3Int cell)
    {
        SetPreviewMaterial(go, world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), building) ? OkMaterial : NoMaterial);
    } 

    public Vector2Int[] OccupiedCells (Vector3Int pos)
    {
        Vector2Int[] occupiedCells = new Vector2Int[building.cells.x * building.cells.y];
        int index = 0;
        for (int x = 0; x < building.cells.x; x++)
        {
            for (int y = 0; y < building.cells.y; y++)
            {
                occupiedCells[index++] = new Vector2Int(pos.x + x, pos.y + y);
            }
        }

        return occupiedCells;
    }

    protected Vector3 GetPlacement(Vector3Int cell)
    {
        Vector3 snapped = grid.CellToWorld(cell);
        Vector3 offset = new Vector3(
            building.cells.x / 2f,
            building.cells.y / 2f,
            -3 // z offset to make the building actually show up
        );

        return snapped + offset;
    }
}