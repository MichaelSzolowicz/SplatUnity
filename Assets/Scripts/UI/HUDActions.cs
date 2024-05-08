using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDActions : UIActions
{
    [SerializeField] protected TMP_Text scoreText, numBalloons;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString("000");
    }

    public void SetNumBalloons(int num)
    {
        numBalloons.text = num.ToString("000");
    }
}
