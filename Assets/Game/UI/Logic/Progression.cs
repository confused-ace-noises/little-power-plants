using System;
using System.Collections;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressionButton : MonoBehaviour
{
    [SerializeField] public SpawnNewTypeButtons.ButtonType type;
    Button button;
    public DenyColor denyColor;

    [DoNotSerialize] private ColorBlock okay;

    [Header("Costs")]
    public float money;

    // bool isUnlocked = false; 

    public ProgressionButton prerequisite;

    public event Action gotUnlocked;


    void Awake()
    {
        button = this.GetComponent<Button>();
        okay = button.colors;
        
        if (prerequisite != null)
        {
            prerequisite.gotUnlocked += onUnlockPrevious;
            button.interactable = false;
        }
    }


    public void OnCLick()
    {
        if (Money.Instance.OneOff(-money))
        {
            SpawnNewTypeButtons.Instance.SpawnButton(type);
            button.interactable = false;
            // isUnlocked = true;
            gotUnlocked?.Invoke();
            // = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        } else
        {
            DenyLoop();
        }
    }

    static int delaySecs = 1;
    IEnumerator DenyLoop()
    {
        Debug.Log("doing deny loop");
        for (int i = 0; i < 3; i++)
        {
            button.colors = denyColor.deny;  
            yield return new WaitForSeconds(delaySecs);
            button.colors = okay;
            yield return new WaitForSeconds(delaySecs);
        }

        button.colors = okay; // to never get them stuck on deny
    }

    public void onUnlockPrevious()
    {
        button.interactable = true;
    }
}