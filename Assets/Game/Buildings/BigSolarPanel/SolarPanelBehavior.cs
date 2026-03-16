using UnityEngine;

[CreateAssetMenu]
public class SolarPanelBehavior : BuildingBehavior
{
    public override BuildingPlacer GetPlacer() => new BuildingPlacer();
}
