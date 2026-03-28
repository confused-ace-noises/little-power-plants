using UnityEngine;

[CreateAssetMenu]
public class CoalPlantBehavior : BuildingBehavior
{
    public float energyOutputCoal;
    public int ecologyCost;
    public int moneyCost;
    public override BuildingData GetData() => new BuildingData(energyOutputCoal, ecologyCost, -moneyCost);

    public override float EnergyProduction() => energyOutputCoal;


    public override BuildingPlacer GetPlacer() => new BuildingPlacer();
}
