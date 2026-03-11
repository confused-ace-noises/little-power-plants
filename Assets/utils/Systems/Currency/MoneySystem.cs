using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public abstract class Currency : MonoBehaviour
{
    public float Amount { get; protected set; }
    protected float ModifyAmount;
    public HashSet<Func<float>> Adders;

    [NonSerialized] public bool isDirty;

    [SerializeField] protected float WaitSecs = 1;

    public Currency(int startAmount)
    {
        this.Amount = startAmount;
        // waitForSeconds = new WaitForSeconds(numSecs);
    }

    void Start()
    {
        StartCoroutine(UpdateTick());
    }

    IEnumerator UpdateTick()
    {
        while (true)
        {
            while(isDirty) {} // wait while is dirty


            if (ModifyAmount != 0)
            {
                Amount += ModifyAmount;
                ValueChanged?.Invoke(Amount);
            }

            if (ThrowGameOver()) {
                GameOver?.Invoke();
                yield break;
            }

            yield return new WaitForSeconds(WaitSecs); 
        }
    }

    public void NewAdder(Func<float> adder)
    {
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
    public event Action<float> ValueChanged;

    [ContextMenu("DebugAddModAmount")]
    private void DebugAddModAmount()
    {
        ModifyAmount += 1;
    } 
}