using UnityEngine;

[CreateAssetMenu]
public class WaveBehavior : BuildingBehavior
{
    public float energyOutputSolar;
    public int ecologyCost;
    public int moneyCost;
    public override BuildingData GetData() => new BuildingData(energyOutputSolar, ecologyCost, -moneyCost);

    public override bool CanBePlacedOnThis(Tile.TileType type)
    {
        return type switch
        {
            Tile.TileType.Water => true,
            _ => false,
        };
    }

    public override float EnergyProduction() => energyOutputSolar;

    public override BuildingPlacer GetPlacer() => new BuildingPlacer();
}
