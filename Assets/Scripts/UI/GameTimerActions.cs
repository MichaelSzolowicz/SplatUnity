using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameTimerActions : MonoBehaviour
{
    [SerializeField] protected TMP_Text timerText;

    protected BalloonsGameMode gameMode;

    protected void Start()
    {
        gameMode = BalloonsGameMode.Instance;

        StartCoroutine(UpdateTimer());
    }

    protected IEnumerator UpdateTimer()
    {
        while (gameMode.IsGameRunning)
        {
            TimeSpan gameTimeSpan = System.TimeSpan.FromSeconds(gameMode.ElapsedGameTime);

            timerText.text = string.Format("{0:D2}:{1:D2}", gameTimeSpan.Minutes, gameTimeSpan.Seconds);
            yield return new WaitForSeconds(1f);
        }
    }
}
