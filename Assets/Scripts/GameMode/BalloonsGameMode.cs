using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonsGameMode : Singleton<BalloonsGameMode>
{
    [SerializeField] protected float timeLimitMinutes, timeLimitSeconds;

    protected void Start()
    {
        // Convert all time limits to seconds.
        timeLimitSeconds = timeLimitSeconds + (timeLimitMinutes * 60);
    }
}
