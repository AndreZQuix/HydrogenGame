using System;
using UnityEngine;

public class Cooldown
{
    private float Value;

    private float TimesUp;
    public bool IsReady => TimesUp <= Time.time;

    public Cooldown(float value)
    {
        Value = value;
    }

    public void Reset()
    {
        TimesUp = Time.time + Value;
    }
}
