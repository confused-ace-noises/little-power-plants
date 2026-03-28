using UnityEngine;

[CreateAssetMenu]
public class BiomassBehavior : BuildingBehavior
{
    public float energyOutputSolar;
    public int ecologyCost;
    public int moneyCost;
    public override BuildingData GetData() => new BuildingData(energyOutputSolar, ecologyCost, -moneyCost);

    public override float EnergyProduction() => energyOutputSolar;

    public override BuildingPlacer GetPlacer() => new BuildingPlacer();
}
