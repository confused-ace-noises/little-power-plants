using UnityEngine;

[System.Serializable]
public class ProgressionNode
{
    public string nodeName;                 // Name or ID
    public Sprite icon;                     // Icon for UI
    public int cost;                        // How much it costs to unlock
    public bool unlocked = false;           // State

    public ProgressionNode[] prerequisites; // Nodes that must be unlocked first
}

[CreateAssetMenu(fileName = "ProgressionTree", menuName = "Game/ProgressionTree")]
public class ProgressionTree : ScriptableObject
{
    public ProgressionNode[] progressionNodes;
}