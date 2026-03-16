using System;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu]
public class EnergyTowerBehavior : BuildingBehavior
{
    public GameObject LinePrefab;
    public override BuildingPlacer GetPlacer() => new EnergyTowerPlacer(LinePrefab);
}

public class EnergyTowerPlacer : BuildingPlacer
{
    private GameObject linePrefab;
    private LineRenderer lineRenderer;
    private Material finalLineMaterial;
    private Vector3Int? startCell;
    #nullable enable
    private GameObject? line;
    private GameObject? previewPowerTower1;
    private GameObject? previewPowerTower2; 


    public EnergyTowerPlacer(GameObject linePrefab)
    {
        this.linePrefab = linePrefab;
    }

    public override void OnBegin() {
        previewPowerTower1 = GameObject.Instantiate(building.prefab);
        Debug.Log("Finished Begin");


        line = GameObject.Instantiate(linePrefab); // TODO: add prefab
        lineRenderer = line.GetComponent<LineRenderer>();
        finalLineMaterial = lineRenderer.material;
        lineRenderer.material = OkMaterial;
        // lineRenderer.positionCount = 2;

        // maybe change it to get the renderer component and set that to off if shit breaks
        previewPowerTower1.SetActive(false); 
    }

    public override void OnHover(Vector3Int cell)
    {
        previewPowerTower1!.SetActive(true); 

        if (startCell != null) // first one was already placed, change cable & move the preview2
        {
            previewPowerTower2?.SetActive(true);
            line?.SetActive(true);

            // move power tower
            previewPowerTower2!.transform.position = GetPlacement(cell);

            var mat = world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), building) ? OkMaterial : NoMaterial;

            // SetPreviewMaterial(previewPowerTower1!, mat); // need to see if this is makes it better or worse
            SetPreviewMaterial(previewPowerTower2!, mat); 
            lineRenderer.material = mat;

            // line
            lineRenderer.SetPosition(1, cell);
        } else
        {
            previewPowerTower1!.transform.position = GetPlacement(cell);
            SetPreviewMaterialSmart(previewPowerTower1!, cell);
        }
        // redraw cable preview from startCell to cell
    }

    public override bool OnConfirm(Vector3Int cell)
    {
        if (startCell == null)
        {
            if (!world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), building)) {
                runner.StartCoroutine(BlinkRoutine(previewPowerTower1!, 0.1f));
                return false;
            }

            startCell = cell; // first click -> set anchor
            previewPowerTower1!.transform.position = GetPlacement(cell);

            previewPowerTower2 = GameObject.Instantiate(building.prefab);
            SetPreviewMaterialSmart(previewPowerTower2, cell);

            lineRenderer.SetPosition(0, cell);
            previewPowerTower2.SetActive(false);
            
            line?.SetActive(false);

            return false; // not done yet
        } else
        {
            if (!world.worldTiles.CanYouPlaceHere(OccupiedCells(cell), building)) {
                // runner.StartCoroutine(BlinkRoutine(previewPowerTower1!, 0.1f));
                runner.StartCoroutine(BlinkRoutine(previewPowerTower2!, 0.1f));
                return false;
            }

            // second click -> actually place
            SetPreviewMaterial(previewPowerTower1!, building.standardMaterial);
            SetPreviewMaterial(previewPowerTower2!, building.standardMaterial);
            lineRenderer.material = finalLineMaterial;

            InvokeBuiildingGotPlaced(building, previewPowerTower1!, OccupiedCells(startCell.Value));
            InvokeBuiildingGotPlaced(building, previewPowerTower2!, OccupiedCells(cell));

            // reset
            previewPowerTower1 = null;
            previewPowerTower2 = null;
            line = null;
            lineRenderer = null;
            startCell = null;
            runner = null;

            return true; // done
        }

    }

    public override void OnCancel()
    {
        GameObject.Destroy(line);
        startCell = null;
    }

    protected bool IsOverOtherTower(Vector2Int cell) 
    {
        if (world.worldTiles.LoadedTileDict.TryGetValue(cell, out Tile tile))
        {
            var occ = tile.Occupier();

            if (occ != null)
            {
                (Building b, GameObject _) = occ.Value;

                if (b.buildingBehavior is EnergyTowerBehavior)
                {
                    return true;
                }
            }
        }

        return false;
    }
}