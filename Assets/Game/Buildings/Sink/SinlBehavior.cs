using UnityEngine;

[CreateAssetMenu]
public class SinkBehavior : BuildingBehavior
{
    public float energyOutputSink;
    public int ecologyBoost;
    public int moneyCost;
    public override BuildingData GetData() => new BuildingData(energyOutputSink, ecologyBoost, -moneyCost);


    public override float EnergyProduction() => energyOutputSink;

    public override BuildingPlacer GetPlacer() => new BuildingPlacer();
}
