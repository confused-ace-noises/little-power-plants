using System;
using UnityEngine;

public class MakeButtonEventEscapePrefab : MonoBehaviour
{
    public Building building;
    
    public void OnCLick()
    {
        clicked?.Invoke(building);
    }

    public event Action<Building> clicked;
}