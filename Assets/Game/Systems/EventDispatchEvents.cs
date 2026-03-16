using System;
using System.Collections;
using UnityEngine;

public class EventDispatch : MonoBehaviour
{
    private const int WaitSecs = 1;

    protected static Event[] eventList = new Event[] {};

    void Start()
    {
        StartCoroutine(UpdateTick());
    }

    IEnumerator UpdateTick()
    {
        while (true)
        {
            if (UnityEngine.Random.value == 0)
            foreach (Event @event in eventList)
            {
                @event.MaybeApplyEvent();
            }

            yield return new WaitForSeconds(WaitSecs); 
        }
    }

    public event Action<int> CallBack;
}

public abstract class Event
{
    public void MaybeApplyEvent()
    {
        if (UnityEngine.Random.value == 0)
        {
            ApplyEvent();
        }
    }

    public abstract void ApplyEvent();
}

