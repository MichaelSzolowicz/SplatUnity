using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonsGameMode : Singleton<BalloonsGameMode>
{
    protected float elapsedGameTime;
    public float ElapsedGameTime { get { return elapsedGameTime; } }    

    protected bool isGameRunning;
    public bool IsGameRunning {  get { return isGameRunning; } }    

    protected void Start()
    {
        InitGame();
    }

    protected void InitGame()
    {
        elapsedGameTime = 0;

        isGameRunning = true;

        StartCoroutine(UpdateTimer());
    }

    protected IEnumerator UpdateTimer()
    {
        while (isGameRunning)
        {
            elapsedGameTime += 1.0f;
            yield return new WaitForSeconds(1);
        }
    }
}
