using System.Linq;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileProvider", menuName = "2D/TileProvider")]
public class TileProvider : ScriptableObject
{
    [SerializeField] TileBase[] tiles;

    #nullable enable
    public TileBase? GetTile(string name)
    {
        TileBase[] tileBases = tiles.AsEnumerable().Where(t => t.name == name).ToArray();
        
        int len = tileBases.Length;

        if (len == 0)
        {
            Debug.LogError($"couldn't find tile with name \'{name}\'");
            return null;
        } else if (len > 1)
        {
            Debug.LogWarning($"found multiple tiles with name \'{name}\', using first found.");
            return tileBases[0];
        } else
        {
            return tileBases[0];
        }
    }
}