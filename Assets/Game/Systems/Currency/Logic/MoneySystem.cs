using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Toolbars;
using UnityEngine;

public abstract class Currency : MonoBehaviour
{
    public float Amount { get; protected set; }
    protected float ModifyAmount;
    [NonSerialized] public HashSet<Func<float>> Adders;

    [NonSerialized] public bool isDirty;

    [SerializeField] protected float WaitSecs = 4;

    

    public Currency(int startAmount)
    {
        this.Amount = startAmount;
        // waitForSeconds = new WaitForSeconds(numSecs);
    }

    void Awake()
    {
        if (Adders == null)
            Adders = new();
        Amount = 0;
        isDirty = false;
        ModifyAmount = 0;
    }

    void Start()
    {
        StartCoroutine(UpdateTick());
    }

    IEnumerator UpdateTick()
    {
        while (true)
        {
            yield return new WaitWhile(() => isDirty); // wait while is dirty

            // TODO: FIXME: check if this actually works
            foreach (Func<float> adder in Adders)
            {
                Amount += adder();
            }
            
            if (Adders.Count != 0)
            {
                ValueChangedTo?.Invoke(Amount);
            }

            if (ThrowGameOver()) {
                GameOver?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(WaitSecs); 
        }
    }

    public bool OneOff(float modifyAmount)
    {
        if (Amount < modifyAmount)
        {
            return false;
        } else
        {
            Amount += modifyAmount;
            return true;
        }
    }


    public void NewAdder(Func<float> adder)
    {
        if (Adders == null)
            Adders = new();
        
        Debug.Log(Adders);
        Debug.Log(adder);
        
        Adders.Add(adder);

        ModifyAmount += adder();
    }

    public void RemoveAdder(Func<float> adder)
    {
        Adders.Remove(adder);

        ModifyAmount -= adder();
    }

    public void SetDirty()
    {
        isDirty = true;
        
        Amount = 0;

        foreach(Func<float> adder in Adders)
        {
            Amount += adder();
        }
    }

    protected abstract bool ThrowGameOver();
    public event Action GameOver;
    public event Action<float> ValueChangedTo;

    [ContextMenu("DebugAddModAmount")]
    private void DebugAddModAmount()
    {
        ModifyAmount += 1;
    } 
}