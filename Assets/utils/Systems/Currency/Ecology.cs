using System.Diagnostics;
using UnityEngine;

public class Ecology : Currency
{
    public float minAmount = 0f;

    // in seconds
    public float maxElapsed = 100f;

    #nullable enable
    public Stopwatch? Doom;

    public Ecology(int startAmount) : base(startAmount)
    {
        Doom = null;
    }

    protected override bool ThrowGameOver()
    {
        if (Amount < minAmount)
        {
            if (Doom != null && Doom.Elapsed.Seconds > maxElapsed)
            {
                return true;
            } else
            {
                Doom = new Stopwatch();
            }
        }

        return false;
    }
}