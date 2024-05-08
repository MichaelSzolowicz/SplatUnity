using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDActions : MonoBehaviour
{
    [SerializeField] protected TMP_Text scoreText, numBalloons;

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void SetNumBalloons(int num)
    {
        numBalloons.text = num.ToString();
    }
}
