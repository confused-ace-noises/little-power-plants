using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class SpawnNewTypeButtons : MonoBehaviour
{
    public PrefabDropDown buttonsPrefabs; 

    public enum ButtonType
    {
        solar
    }

    public void SpawnButton(ButtonType type)
    {
        Instantiate(GetPrefab(type), transform);
    }

    GameObject GetPrefab(ButtonType type)
    {
        return type switch
        {
            ButtonType.solar => buttonsPrefabs.solar,
            _ => throw new Exception("This shouldn't happen - getPrefab - SpawnNewTypeButtons"),
        };
    }


    [ContextMenu("DebugSpawnSolar")]
    private void DebugSpawnSolar()
    {
        SpawnButton(ButtonType.solar);     
    }
}

[System.Serializable]
public class PrefabDropDown
{
    public GameObject solar;
}