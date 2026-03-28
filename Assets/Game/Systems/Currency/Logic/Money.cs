using UnityEngine;

public class Money : Currency
{
    public static Money Instance;

    void Awake()
    {
        if (Instance != null)
        {
            GameObject.Destroy(Instance);
        }

        Instance = this;
    }

    public float minAmount = 0f;



    public Money(int startAmount) : base(startAmount)
    {
        // numSecs = 1f;
    }

    protected override bool ThrowGameOver() => Amount < minAmount;
}