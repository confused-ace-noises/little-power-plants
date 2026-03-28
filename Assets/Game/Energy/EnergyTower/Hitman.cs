using System.Collections.Generic;
using UnityEngine;

public class Hitman : MonoBehaviour
{
    #nullable enable
    public List<GameObject>? destroyToo = null;

    public void Add(GameObject toDestroy)
    {
        if (destroyToo == null)
        {
            destroyToo = new List<GameObject>();
        }
        
        destroyToo.Add(toDestroy);
    }


    void OnDestroy()
    {
        if (destroyToo == null) return;
        
        foreach (GameObject GO in destroyToo)
        {
            if (GO != null)
            {
                Destroy(GO);
            }
        }

    }
}
