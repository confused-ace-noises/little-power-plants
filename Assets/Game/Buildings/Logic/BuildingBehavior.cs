using Unity.VisualScripting;
using UnityEngine;

// [System.Serializable]
public abstract class BuildingBehavior : ScriptableObject
{
    public abstract BuildingPlacer GetPlacer();

    public virtual bool CaresIfOccupied((Building, GameObject) occupiedBy)
    {
        return true;
    }

    public virtual bool CanBePlacedOnThis(Tile.TileType type)
    {
        return type switch
        {
            Tile.TileType.Grass or Tile.TileType.Sand => true,
            Tile.TileType.Water or Tile.TileType.Mountain => false,
            _ => throw new System.Exception("The tile types are fucked up.")
        };
    }

    public Vector3 placementOffset;

    public abstract float EnergyProduction();

    public abstract BuildingData GetData();
}

public class BuildingData
{
    
    public float EnergyProd { get; private set; }
    public int EcologyProd { get; private set; }
    public int Money { get; private set; }

    public BuildingData(float energy, int ecology, int money)
    {
        EnergyProd = energy;
        EcologyProd = ecology;
        Money = money;
    }

    public BuildingData GetDestroyValues() => new BuildingData(-EnergyProd, -EcologyProd, Money);
}