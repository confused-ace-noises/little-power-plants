using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Compatibility;
using UnityEngine;

public class SpawnNewTypeButtons : MonoBehaviour
{
    public PrefabDropDown buttonsPrefabs; 

    public event Action<GameObject> GO_GotPlaced;

    public enum ButtonType
    {
        Solar,
        PowerTower,
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
            ButtonType.Solar => buttonsPrefabs.solar,
            ButtonType.PowerTower => buttonsPrefabs.powerTower,
            _ => throw new Exception("This shouldn't happen - getPrefab - SpawnNewTypeButtons"),
        };
    }


    [ContextMenu("DebugSpawnSolar")]
    private void DebugSpawnAll()
    {
        SpawnButton(ButtonType.Solar);     
        SpawnButton(ButtonType.PowerTower);     
    }
}

[System.Serializable]
public class PrefabDropDown
{
    public GameObject solar;
    public GameObject powerTower;
}