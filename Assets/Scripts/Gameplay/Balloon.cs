using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    private static int numBalloons;
    public static int NumBalloons {  get { return numBalloons; } }

    [SerializeField] protected int pointValue;

    protected void Awake()
    {
        numBalloons++;
    }

    protected void OnTriggerEnter(Collider other)
    {
        Projectile projectile = other.GetComponent<Projectile>();
        if(projectile != null)
        {
            Debug.Log(name + " popped by " + projectile.Instigator.name);

            projectile.Instigator.AddScore(pointValue);

            gameObject.SetActive(false);
        }
    }

    protected void OnDestroy()
    {
        numBalloons--;
    }
}
