using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

// FOR THE LOVE OF ALL THAT IS HOLY AND UNHOLY, 
// DO *NOT* TOUCH THIS FILE

public class BuildingPlacementSystem : MonoBehaviour
{
    public static event Action<Building> StartingToPlace;
    public static event Action<Building> CanceledPlacing;
    public static event Action<Building, GameObject, HashSet<Vector2Int>> BuildingGotPlaced;
    public static event Action<Building, HashSet<Vector2Int>> BuildingGotRemoved;

    public bool IsRemoveMode { get; private set; }
    
    #nullable enable
    private GameObject? hoveredGO;
    private Building? hoveredBuilding;


    public SpawnNewTypeButtons spawnNewTypeButtons;

    [Header("WorldInfo")]
    [SerializeField] Grid grid;
    [SerializeField] World world;
    [SerializeField] private TileIsBeingSelected tileIsBeingSelected;
    
    [Header("Materials")]
    [SerializeField] private Material translucentMaterialOk;
    [SerializeField] private Material translucentMaterialNo;
    [SerializeField] private Material translucentMaterialRemove;

    [Header("Debug")]
    [SerializeField] private Building debugBuilding;
    
    
    #nullable enable
    private BuildingPlacer? currentPlacer;

    public static BuildingPlacementSystem Instance { get; private set; }

    void Awake() {
        if (Instance != null)
        {
            GameObject.Destroy(Instance);
        }
        Instance = this;
    }

    void Start()
    {
        IsRemoveMode = false;
        MouseTrackSystem.MouseMovedIntoCell += OnMouseMovedIntoCell;
        // MouseTrackSystem.MouseMovedOutOfCell += OnMouseMovedOutOfCell;
        MouseTrackSystem.NewMouseStateInCell += TryPlaceOrRemoveBuilding;
        spawnNewTypeButtons.GO_GotPlaced += SubscribeToNewButton;
        spawnNewTypeButtons.SetUp();
    }

    public void ChangeMode()
    {
        IsRemoveMode = !IsRemoveMode;
 
        if (IsRemoveMode)
        {
            if (currentPlacer != null)
            {
                currentPlacer.OnCancel();
                CanceledPlacing?.Invoke(currentPlacer.building);
                DetachPlacer();
            }
            tileIsBeingSelected.SetOverrideTrue();
        }
        else
        {
            ClearRemoveHover();
            tileIsBeingSelected.SetOverrideFalse();
        }
    }

    public void ChangeMode(InputAction.CallbackContext callbackContext)
    {
        IsRemoveMode = !IsRemoveMode;
 
        if (IsRemoveMode)
        {
            if (currentPlacer != null)
            {
                currentPlacer.OnCancel();
                CanceledPlacing?.Invoke(currentPlacer.building);
                DetachPlacer();
            }
            tileIsBeingSelected.SetOverrideTrue();
        }
        else
        {
            ClearRemoveHover();
            tileIsBeingSelected.SetOverrideFalse();
        }
    }

    private void TryRemoveBuilding(Vector3Int cell)
    {
        if (!TryGetOccupier(cell, out BuildingOnTileInfo? buildingOnTileInfo))
            return;
 
        // Drop hover state before destroying the GO.
        hoveredGO = null;
        hoveredBuilding = null;

        // Tell every tile in the family it is no longer occupied.
        foreach (var occupiedCell in buildingOnTileInfo.Value.family)
        {
            if (world.worldTiles.LoadedTileDict.TryGetValue(occupiedCell, out Tile t))
                t.SetOccupiedFalse();
        }
 
        Destroy(buildingOnTileInfo.Value.gameObject);
 
        BuildingGotRemoved?.Invoke(buildingOnTileInfo.Value.building, buildingOnTileInfo.Value.family);
    }


    private void UpdateRemoveHover(Vector3Int cell)
    {
        if (!TryGetOccupier(cell, out BuildingOnTileInfo? buildingOnTileInfo))
        {
            ClearRemoveHover();
            return;
        }

        GameObject go = buildingOnTileInfo.Value.gameObject;

        if (go == hoveredGO) return;

        ClearRemoveHover();

        hoveredGO = go;
        hoveredBuilding = buildingOnTileInfo!.Value.building;

        foreach (var r in go.GetComponentsInChildren<Renderer>())
            r.material = translucentMaterialRemove;
    }

    private void ClearRemoveHover()
    {
        if (hoveredGO == null) return;

        foreach (var r in hoveredGO.GetComponentsInChildren<Renderer>())
            r.material = hoveredBuilding.standardMaterial;

        hoveredGO = null;
        hoveredBuilding = null;
    } 

    public void SubscribeToNewButton(GameObject GO)
    {
        /*
        if (currentPlacer != null)
        {
            currentPlacer.OnCancel();
            DetachPlacer();
        }
        */

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
        if (IsRemoveMode) ChangeMode();

        StartingToPlace?.Invoke(building);

        tileIsBeingSelected.SetOverrideTrue();

        if (currentPlacer != null)
        {
            currentPlacer.OnCancel();
            CanceledPlacing?.Invoke(currentPlacer.building);
            DetachPlacer();
        }

        currentPlacer = building.buildingBehavior.GetPlacer();
        currentPlacer.BuildingGotPlaced += PropagateBuildingPlaced;

        currentPlacer.Initialize(building, grid, world, translucentMaterialOk, translucentMaterialNo, this);
        currentPlacer.SetUpRun(this);
        currentPlacer.OnBegin();
    }

    private void OnMouseMovedIntoCell(Vector3Int cell)
    {
        if (IsRemoveMode)
        {
            UpdateRemoveHover(cell);
            return;
        }

        if (currentPlacer == null) return;

        currentPlacer.OnHover(cell);
    }
    
    public void TryPlaceOrRemoveBuilding(Vector3Int cell, bool mouseDown)
    {
        if (!mouseDown) return;
 
        if (IsRemoveMode)
        {
            TryRemoveBuilding(cell);
            return;
        }
 
        if (currentPlacer == null) return;
 
        bool placed = currentPlacer.OnConfirm(cell);
        if (placed)
        {
            tileIsBeingSelected.SetOverrideFalse();
            DetachPlacer();
        }
    }

    public void OnEscape(InputAction.CallbackContext callbackContext)
    {
        if (IsRemoveMode) ChangeMode();

        if (currentPlacer == null) return;

        currentPlacer.OnCancel();
        CanceledPlacing?.Invoke(currentPlacer.building);
        DetachPlacer();
    }

    public void ForcePlace(Building building, Vector3Int position)
    {
        var tmpPlacer = currentPlacer;
        if (currentPlacer != null)
        {
            DetachPlacer(); // we temporarily detach the placer to use the forced one
        }

        AttachPlacer(building);

        currentPlacer.Initialize(building, grid, world, translucentMaterialOk, translucentMaterialNo, this);
        currentPlacer.SetUpRun(this);
        currentPlacer.FastPlace(position); // ...pray this thing never throws

        DetachPlacer();
        if (tmpPlacer != null)
        {
            AttachPlacer(tmpPlacer); // if the old placer wasn't null, we put it back
        }
    }

    public PendingBuilding ForcePlaceMany(Building building, Vector3Int[] positions)
    {
        var tmpPlacer = currentPlacer;
        if (currentPlacer != null)
        {
            DetachPlacer(); // we temporarily detach the placer to use the forced one
        }

        AttachPlacer(building);

        currentPlacer.Initialize(building, grid, world, translucentMaterialOk, translucentMaterialNo, this);
        currentPlacer.SetUpRun(this);
        PendingBuilding pending = currentPlacer.FastPlaceMany(positions); // ...pray this thing never throws

        DetachPlacer();
        if (tmpPlacer != null)
        {
            AttachPlacer(tmpPlacer); // if the old placer wasn't null, we put it back
        }

        return pending;
    }

    private void DetachPlacer()
    {
        currentPlacer.BuildingGotPlaced -= PropagateBuildingPlaced;
        currentPlacer = null;
    }

    private void AttachPlacer(Building building)
    {
        currentPlacer = building.buildingBehavior.GetPlacer();
        currentPlacer.BuildingGotPlaced += PropagateBuildingPlaced;
    }

    private void AttachPlacer(BuildingPlacer placer)
    {
        currentPlacer = placer;
        currentPlacer.BuildingGotPlaced += PropagateBuildingPlaced;
    }

    public static void PropagateBuildingPlaced(Building building, GameObject GO, HashSet<Vector2Int> pos)
    {
        BuildingGotPlaced?.Invoke(building, GO, pos);
    }

    private bool TryGetOccupier(Vector3Int cell, out BuildingOnTileInfo? buildingInfo)
    {
        buildingInfo = null;

        var cell2D = new Vector2Int(cell.x, cell.y);
        bool exists = world.worldTiles.LoadedTileDict.TryGetValue(cell2D, out Tile tile);
        if (!exists) return false;
 
        var occupier = tile.Occupier();
        if (!occupier.HasValue) return false;
 
        buildingInfo = occupier.Value;
        return true;
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

    public event Action<Building, GameObject, HashSet<Vector2Int>> BuildingGotPlaced;

    protected void InvokeBuildingGotPlaced(Building building, GameObject GO, HashSet<Vector2Int> pos)
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
        // this is quite sketchy, but it's here to prevent that the incredibly sketchy
        // FastPlace method throws a nullRef exception in some corner cases  
        if (preview == null) return; 


        preview.SetActive(true);

        preview.transform.position = GetPlacement(cell);

        SetPreviewMaterialSmart(preview, cell);
    }

    // Called when the player clicks
    // Returns a GO if placement is complete (and system should exit placement mode)
    // returns null otherwise
    public virtual bool OnConfirm(Vector3Int cell)
    {
        if (!world.worldTiles.CanYouPlaceHere(building.OccupiedCells(cell), building)) {
            Debug.Log($"is runner null: {runner == null}");
            runner.StartCoroutine(BlinkRoutine(preview, 0.1f));
            SetPreviewMaterialSmart(preview, cell);
            return false;
        }

        // Optionally: check if all cells are free (you can hook into your GridManager here)

        // Place the actual building prefab
        // GameObject placedGO = GameObject.Instantiate(building.prefab);
        preview.transform.position = GetPlacement(cell);
        preview.SetActive(true);
        SetPreviewMaterial(preview, building.standardMaterial);

        BuildingGotPlaced?.Invoke(building, preview, building.OccupiedCells(cell).ToHashSet());
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
        if (go.TryGetComponent<Renderer>(out var renderer))
        {
            renderer.material = mat;
        }
    }

    protected void SetPreviewMaterialSmart(GameObject go, Vector3Int cell)
    {
        SetPreviewMaterial(go, world.worldTiles.CanYouPlaceHere(building.OccupiedCells(cell), building) ? OkMaterial : NoMaterial);
    } 

    protected Vector3 GetPlacement(Vector3Int cell)
    {
        Vector3 snapped = grid.CellToWorld(cell);
        Vector3 offset = building.buildingBehavior.placementOffset;

        return snapped + offset;
    }


    /// <summary>
    /// incredibly skecthy, check that you can place there first
    /// </summary>
    /// <param name="cell"></param>
    public virtual void FastPlace(Vector3Int cell)
    {
        var GO = GameObject.Instantiate(building.prefab);
        GO.transform.position = GetPlacement(cell);
        BuildingGotPlaced?.Invoke(building, GO, building.OccupiedCells(cell).ToHashSet());
    }

    public virtual PendingBuilding FastPlaceMany(Vector3Int[] cells)
    {
        Vector3[] cellsNotInt = new Vector3[cells.Length];
        Quaternion[] quaternions = new Quaternion[cells.Length];
        HashSet<Vector2Int>[] positions = new HashSet<Vector2Int>[cells.Length];

        for (int i = 0; i < cells.Length; i++)
        {
            cellsNotInt[i] = GetPlacement(cells[i]);
            quaternions[i] = Quaternion.identity;
            positions[i] = building.OccupiedCells(cells[i]).ToHashSet();
        }


        AsyncInstantiateOperation<GameObject> op = GameObject.InstantiateAsync(building.prefab, cells.Length, cellsNotInt, quaternions);
        return new PendingBuilding(op, building, positions);
    }
}

public class PendingBuilding
{
    readonly AsyncInstantiateOperation<GameObject> op;
    readonly Building building;
    readonly HashSet<Vector2Int>[] occupied;

    public bool isDone;

    public PendingBuilding(AsyncInstantiateOperation<GameObject> op, Building building, HashSet<Vector2Int>[] occupied, float timeoutSeconds = 5f)
    {
        isDone = false;

        this.op = op;
        this.building = building;
        this.occupied = occupied;

        _ = AwaitWithTimeout(timeoutSeconds);
    }

    async Task AwaitWithTimeout(float seconds)
    {
        async Task WaitForOp() => await op;

        var _ = await Task.WhenAny(WaitForOp(), Task.Delay(TimeSpan.FromSeconds(seconds)));

        if (!op.isDone)
            op.WaitForCompletion(); // force sync finish if timed out

        isDone = true;   
        Propagate();
    }

    void Propagate()
    {
        for (int i = 0; i < op.Result.Length; i++)
            BuildingPlacementSystem.PropagateBuildingPlaced(building, op.Result[i], occupied[i]);
    }
}