using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Compatibility;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnNewTypeButtons : MonoBehaviour
{
    public static SpawnNewTypeButtons Instance; 

    public PrefabDropDown buttonsPrefabs; 

    public event Action<GameObject> GO_GotPlaced;

    [Serializable]
    public enum ButtonType
    {
        SolarSmall,
        SolarMedium, 
        SolarBig,
        
        SmallGeo,
        BigGeo,

        Nuclear,

        EolicSmall,
        EolicBig,

        WaveElectricity,
        
        
        PowerTower,
        Coal,
    }

    void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(Instance);
        }

        Instance = this;
    }

    public void SetUp()
    {
        SpawnButton(ButtonType.PowerTower);
    }

    public void SpawnButton(ButtonType type)
    {
        GameObject x = Instantiate(GetPrefab(type), transform);

        GO_GotPlaced?.Invoke(x);
    }

    GameObject GetPrefab(ButtonType type)
    {
        return type switch
        {
            ButtonType.SolarSmall => buttonsPrefabs.solarSmall,
            ButtonType.SolarMedium => buttonsPrefabs.solarMedium, 
            ButtonType.SolarBig => buttonsPrefabs.solarBig, 
            
            ButtonType.SmallGeo => buttonsPrefabs.smallGeo, 
            ButtonType.BigGeo => buttonsPrefabs.bigGeo, 
            
            ButtonType.Nuclear => buttonsPrefabs.nuclear, 

            ButtonType.EolicBig => buttonsPrefabs.eolicBig, 
            ButtonType.EolicSmall => buttonsPrefabs.eolicSmall, 
            
            ButtonType.WaveElectricity => buttonsPrefabs.waveElectricity, 

            ButtonType.PowerTower => buttonsPrefabs.powerTower,
            ButtonType.Coal => buttonsPrefabs.coal,
            _ => throw new Exception("This shouldn't happen - getPrefab - SpawnNewTypeButtons"),
        };
    }


    [ContextMenu("DebugSpawnAll")]
    private void DebugSpawnAll()
    {
        // FIXME
        SpawnButton(ButtonType.SolarSmall);     
        SpawnButton(ButtonType.SolarMedium);     
        SpawnButton(ButtonType.SolarBig);     
        
        SpawnButton(ButtonType.EolicBig);     
        SpawnButton(ButtonType.EolicSmall);     
        
        SpawnButton(ButtonType.WaveElectricity);
        
        SpawnButton(ButtonType.SmallGeo);     
        SpawnButton(ButtonType.BigGeo);     
        
        SpawnButton(ButtonType.Nuclear);     

        // SpawnButton(ButtonType.PowerTower);
        SpawnButton(ButtonType.Coal); 
    }
}

[System.Serializable]
public class PrefabDropDown
{
    public GameObject solarSmall;
    public GameObject solarMedium;
    public GameObject solarBig;

    public GameObject smallGeo;
    public GameObject bigGeo;

    public GameObject nuclear;

    public GameObject eolicSmall;
    public GameObject eolicBig;

    public GameObject waveElectricity;

    public GameObject powerTower;
    public GameObject coal;
}