using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Defines and manages current player state.
/// </summary>
[RequireComponent(typeof(PlayerController))]
public class Player : PlayerStub
{
    public Shooter weapon;
    public HUDActions HUD;

    protected PlayerController controller;

    protected int score;

    protected void Start()
    {
        weapon.instigator = this;

        controller = GetComponent<PlayerController>();
        controller.Shooter = weapon;

        HUD.SetScore(score);
        HUD.SetNumBalloons(Balloon.NumBalloons);
    }

    /// <summary>
    /// Add points to the player's current score.
    /// </summary>
    /// <param name="pointValue"></param>
    public void AddScore(int pointValue)
    {
        Debug.Log(name + " received " + pointValue + " points.");

        score += pointValue;

        HUD.SetScore(score);
    }
}
