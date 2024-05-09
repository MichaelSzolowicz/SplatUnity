using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RedBalloon : Balloon
{
    private static int numBalloons;
    private BalloonChainInfoStruct chainInfo;

    [SerializeField] protected int pointValue;

    protected void Awake()
    {
        chainInfo = new BalloonChainInfoStruct();
        chainInfo.color = BalloonColor.Red;

        chainInfo.numRemainingBalloons = ++numBalloons;
    }

    protected void OnTriggerEnter(Collider other)
    {
        Projectile projectile = other.GetComponent<Projectile>();
        if (projectile != null)
        {
            Debug.Log(name + " popped by " + projectile.Instigator.name);

            projectile.Instigator.AddScore(pointValue);

            gameObject.SetActive(false);
        }
    }

    protected void OnDisable()
    {
        chainInfo.numRemainingBalloons = --numBalloons;
    }
}
