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

    protected PlayerController controller;

    protected void Start()
    {
        weapon.instigator = this;

        controller = GetComponent<PlayerController>();
        controller.Shooter = weapon;
    }

    /// <summary>
    /// Add points to the player's current score.
    /// </summary>
    /// <param name="pointValue"></param>
    public void AddScore(int pointValue)
    {
        Debug.Log(name + " received " + pointValue + " points.");
    }
}
