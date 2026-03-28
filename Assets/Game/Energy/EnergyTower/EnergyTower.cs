using System;
using System.Collections.Generic;
using Unity.Profiling;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// TODO: fix materials on hover

[CreateAssetMenu]
public class EnergyTowerBehavior : BuildingBehavior
{
    private bool isDataStale = false;
    public override BuildingData GetData()
    {
        var (distance, hasPlaced) = EnergyTowerPlacer.distance;
        
        int money;

        if (distance != null && hasPlaced && !isDataStale) // got placed, we didn't read from it yet.
        {
            isDataStale = true;
            money = CalcMoney(distance.Value);
        } else if (distance != null && hasPlaced && isDataStale) // data is stale, return 0 i guess? who would even call this when it's stale?
        {
            money = 0;
        } else if (distance != null && !hasPlaced)
        {
            isDataStale = false; // data won't ever be stale while we're placing
            money = CalcMoney(distance.Value);
        } else if (distance == null)
        {
            isDataStale = false; // data won't ever be stale while we're placing (we never set it to null here, so if it's null it means it got reset to null by the placer)
            money = CalcMoney(0); // distance is null, let's just say it's 0
        } else
        {
            throw new NotImplementedException("This brach should be impossible, you fucked something up.");
        }

        return new BuildingData(0, 0, money);
    }

    private int CalcMoney(float dist)
    {
        int moneyPerDist = 10;

        return Mathf.RoundToInt((dist + 1) * moneyPerDist);
    }

    public GameObject LinePrefab;

    public override bool CaresIfOccupied((Building, GameObject) occupiedBy)
    {
        return false; // tower things never care whether they are placed on top of something else
    }

    public override float EnergyProduction() => 0;

    public override BuildingPlacer GetPlacer() => new EnergyTowerPlacer(LinePrefab);
}

public class EnergyTowerPlacer : BuildingPlacer
{
    public static (float?, bool) distance = (null, false);

    static readonly Vector3 lineOffset = new Vector3(x: 0.5f, y: 0.5f, z: 0.0f);
    private GameObject linePrefab;
    private LineRenderer lineRenderer;
    private Material finalLineMaterial;
    private Vector3Int? startCell;
    #nullable enable
    private GameObject? line;
    private GameObject? previewPowerTower1;
    private GameObject? previewPowerTower2;

    private bool powerTower1ShouldBePlaced;
    private bool powerTower2ShouldBePlaced;


    public EnergyTowerPlacer(GameObject linePrefab)
    {
        this.linePrefab = linePrefab;
    }

    public override void OnBegin() {
        previewPowerTower1 = GameObject.Instantiate(building.prefab);
        Debug.Log("Finished Begin");


        line = GameObject.Instantiate(linePrefab);
        lineRenderer = line.GetComponent<LineRenderer>();
        finalLineMaterial = lineRenderer.material;
        lineRenderer.material = OkMaterial;
        // lineRenderer.positionCount = 2;

        // maybe change it to get the renderer component and set that to off if shit breaks
        previewPowerTower1.SetActive(false); 
        distance = (null, false);
    }

    public override void OnHover(Vector3Int cell)
    {
        previewPowerTower1?.SetActive(true); 

        if (startCell != null) // first one was already placed, change cable & move the preview2
        {
            previewPowerTower2?.SetActive(true);
            line?.SetActive(true);

            // move power tower
            previewPowerTower2!.transform.position = GetPlacement(cell);


            Material mat = GetMaterialBasedOnBuildingUnderneath(previewPowerTower2, (Vector2Int)cell);
            

            // SetPreviewMaterial(previewPowerTower1!, mat); // need to see if this is makes it better or worse
            SetPreviewMaterial(previewPowerTower2!, mat); 
            lineRenderer.material = mat;

            // line
            lineRenderer.SetPosition(1, ((Vector3)cell) + lineOffset);
            distance = (Vector3Int.Distance(startCell.Value, cell), false);
        } else
        {
            previewPowerTower1!.transform.position = GetPlacement(cell);

            Material mat = GetMaterialBasedOnBuildingUnderneath(previewPowerTower1, (Vector2Int)cell);

            SetPreviewMaterial(previewPowerTower1, mat);
        }
        // redraw cable preview from startCell to cell
    }

    private Material GetMaterialBasedOnBuildingUnderneath(GameObject GO, Vector2Int cell)
    {
        bool exists = world.worldTiles.LoadedTileDict.TryGetValue(cell, out Tile tileImOn);

        if (exists && building.buildingBehavior.CanBePlacedOnThis(tileImOn.type))
        {
            var occ = tileImOn.Occupier();

            if (occ != null && occ.Value.building.buildingBehavior is EnergyTowerBehavior)
            {
                GO.SetActive(false);
                return OkMaterial;
            } else if (occ != null)
            {
                GO.SetActive(false);
                return OkMaterial;
            } else
            {
                GO.SetActive(true);
                return OkMaterial;
            }
        } else
        {
            return NoMaterial;
        }
    }

    private bool ShouldIPlaceIt(Vector2Int cell)
    {
        var exists = world.worldTiles.LoadedTileDict.TryGetValue(cell, out Tile tileImOn);

        if (exists)
        {
            Debug.Log($"returning {tileImOn.Occupier() == null} for cell {cell}");
            return tileImOn.Occupier() == null;
        } else
        {
            return false;
        }
    }

    public override bool OnConfirm(Vector3Int cell)
    {
        if (startCell == null)
        {
            if (!world.worldTiles.CanYouPlaceHere(building.OccupiedCells(cell), building)) {
                runner.StartCoroutine(BlinkRoutine(previewPowerTower1!, 0.1f));
                return false;
            }

            startCell = cell; // first click -> set anchor
            previewPowerTower1!.transform.position = GetPlacement(cell);

            powerTower1ShouldBePlaced = ShouldIPlaceIt((Vector2Int)cell);

            if (!powerTower1ShouldBePlaced) previewPowerTower1.SetActive(false);

            previewPowerTower2 = GameObject.Instantiate(building.prefab);
            SetPreviewMaterialSmart(previewPowerTower2, cell);

            lineRenderer.SetPosition(0, ((Vector3)cell) + lineOffset);
            previewPowerTower2.SetActive(false);
            
            line?.SetActive(false);

            return false; // not done yet
        } else
        {
            if (!world.worldTiles.CanYouPlaceHere(building.OccupiedCells(cell), building)) {
                // runner.StartCoroutine(BlinkRoutine(previewPowerTower1!, 0.1f));
                runner.StartCoroutine(BlinkRoutine(previewPowerTower2!, 0.1f));
                return false;
            }
            
            powerTower2ShouldBePlaced = ShouldIPlaceIt((Vector2Int)cell);

            Debug.Log($"status: 1 -> {powerTower1ShouldBePlaced}, \n2 -> {powerTower2ShouldBePlaced}");

            if (powerTower1ShouldBePlaced)
            {
                DiscoverEnergySystem.registerSingle.Invoke(building.OccupiedCells(startCell.Value).ToHashSet(), building.buildingBehavior);
                
                HireHitman(previewPowerTower1!);
                
                SetPreviewMaterial(previewPowerTower1!, building.standardMaterial);
                InvokeBuildingGotPlaced(building, previewPowerTower1!, building.OccupiedCells(startCell.Value).ToHashSet());
            } else
            {
                // find out whatever it connected to here
                GameObject.Destroy(previewPowerTower1);
            }

            if (powerTower2ShouldBePlaced)
            {
                if (powerTower1ShouldBePlaced)
                {
                    DiscoverEnergySystem.connectNew.Invoke(building.OccupiedCells(startCell.Value).ToHashSet(), building.OccupiedCells(cell).ToHashSet(), building.buildingBehavior);
                } else
                {
                    var exists = world.worldTiles.LoadedTileDict.TryGetValue((Vector2Int)startCell.Value, out Tile tile);
                    
                    if (!exists)
                        throw new Exception("how the hell did you even get here");

                    var occ = tile.Occupier()!.Value;

                    HireHitman(occ.gameObject);

                    DiscoverEnergySystem.connectNew.Invoke(occ.family, building.OccupiedCells(cell).ToHashSet(), building.buildingBehavior);
                }

                HireHitman(previewPowerTower2!);
                SetPreviewMaterial(previewPowerTower2!, building.standardMaterial);
                InvokeBuildingGotPlaced(building, previewPowerTower2!, building.OccupiedCells(cell).ToHashSet());
            } else
            {
                var exists1 = world.worldTiles.LoadedTileDict.TryGetValue((Vector2Int) startCell.Value, out Tile tile1);
                
                
                if (!exists1)
                    throw new Exception("how the hell did you even get here");

                var exists2 = world.worldTiles.LoadedTileDict.TryGetValue((Vector2Int) cell, out Tile tile2);
                    
                if (!exists2)
                    throw new Exception("how the hell did you even get here");

                var occ1 = tile1.Occupier()!.Value;
                var occ2 = tile2.Occupier()!.Value;

                Debug.Log($"GO: {occ1.gameObject}, \n family: {occ1.family} \n building: {occ1.building}");

                HireHitman(occ1.gameObject);
                HireHitman(occ2.gameObject);

                DiscoverEnergySystem.connectExisting.Invoke(occ1.family, occ2.family);
                GameObject.Destroy(previewPowerTower2);
            }


            // DiscoverEnergySystem.
            // EnergyTowerBehavior.newConnection?.Invoke((OccupiedCells(startCell.Value), building.OccupiedCells(cell)), (value1, value2));

            lineRenderer.material = finalLineMaterial;

            distance = (Vector3Int.Distance(startCell.Value, cell), true);

            Reset();

            return true; // done
        }

    }

    private void HireHitman(GameObject hitman)
    {
        Hitman hitmanComponent = hitman.GetOrAddComponent<Hitman>();
        hitmanComponent.Add(line);
    }

    private void Reset()
    {
        previewPowerTower1 = null;
        previewPowerTower2 = null;
        line = null;
        lineRenderer = null;
        startCell = null;
        runner = null;
    }

    public override void OnCancel()
    {
        GameObject.Destroy(previewPowerTower1);
        GameObject.Destroy(previewPowerTower2);
        GameObject.Destroy(line);
        Reset();
    }

    protected bool IsOverOtherTower(Vector2Int cell) 
    {
        if (world.worldTiles.LoadedTileDict.TryGetValue(cell, out Tile tile))
        {
            var occ = tile.Occupier();

            if (occ != null)
            {
                var b = occ.Value.building;

                if (b.buildingBehavior is EnergyTowerBehavior)
                {
                    return true;
                }
            }
        }

        return false;
    }
}