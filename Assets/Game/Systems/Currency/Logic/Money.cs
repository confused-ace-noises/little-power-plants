using UnityEngine;

public class Money : Currency
{
    public float minAmount = 0f;

    public Money(int startAmount) : base(startAmount)
    {
        // numSecs = 1f;
    }

    protected override bool ThrowGameOver() => Amount < minAmount;
}