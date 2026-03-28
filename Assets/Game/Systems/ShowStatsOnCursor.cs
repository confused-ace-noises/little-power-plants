using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ShowStatsOnCursor : MonoBehaviour
{
    [Header("Container")]
    public RectTransform container;
    public Vector2 containerOffset;
    public World world;

    [Header("Elements")]
    public ShowStatsPiece cursorStats;
    public ShowStatsPiece energyStats;
    public ShowStatsPiece moneyStats;
    public ShowStatsPiece ecologyStats;
    public ShowStatsPiece buildingInfo;
    

    public static ShowStatsOnCursor Instance {get; private set;}

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this;
    } 

    void Start()
    {
        Debug.Log("started");
        ClearData();
        MouseTrackSystem.MouseMoved += MouseMoved;
        MouseTrackSystem.MouseMovedIntoCell += MouseMovedIntoCell;
    }

    CostDisplayMode GetMode()
    {
        if (!BuildingPlacementSystem.Instance.IsRemoveMode)
        {
            return CostDisplayMode.Place;
        } else
        {
            return CostDisplayMode.Remove;
        }
    }

    public enum CostDisplayMode
    {
        Place,
        Remove
    }

    public void DisplayStats(Building building, CostDisplayMode mode)
    {
        Debug.Log("idk");
        BuildingData buildingData = mode switch
        {
            CostDisplayMode.Place => building.buildingBehavior.GetData(),
            CostDisplayMode.Remove => building.buildingBehavior.GetData().GetDestroyValues(),
            _ => throw new System.NotImplementedException("this branch should be unreachable"),
        };

        Debug.Log("passed mode");
        moneyStats.Enable($"{buildingData.Money}");
        energyStats.Enable($"{buildingData.EnergyProd}");
        ecologyStats.Enable($"{buildingData.EcologyProd}");
        Debug.Log("passed enable");
        // buildingInfo.SetText($"{buildingData.Money}");
    }

    void MouseMoved(Vector2 pos)
    {
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            container.parent as RectTransform,
            pos,
            null, // null = screen space overlay, pass your camera if world space
            out Vector2 localPoint
        );
        container.localPosition = localPoint + containerOffset;
    }

    void MouseMovedIntoCell(Vector3Int cell)
    {
        cursorStats.Enable($"x: {cell.x}; y: {cell.y}");

        Tile tile = world.worldTiles.GetTile((Vector2Int)cell);
        var occ = tile.Occupier();

        if (occ != null)
        {
            Debug.Log("occ not null");
            var (building, _) = occ.Value.Tuple;
            var mode = GetMode();

            DisplayStats(building, mode);
        } else
        {
            Debug.Log("occ is null");

            ClearData();
        }
    }

    void ClearData()
    {
        cursorStats.Disable();
        energyStats.Disable();
        moneyStats.Disable();
        ecologyStats.Disable();
        buildingInfo.Disable();
    }

}

[Serializable]
public class ShowStatsPiece
{
    public TMP_Text text;
    
    public void Enable(string displayText)
    {
        text.SetText(displayText);
        text.GetComponent<RectTransform>().parent.gameObject.SetActive(true);
        // Debug.Log("enabled");
    }

    public void Disable()
    {
        text.SetText("");
        text.GetComponent<RectTransform>().parent.gameObject.SetActive(false);
        // Debug.Log("disabled");
    }
}