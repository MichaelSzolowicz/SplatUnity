using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Balloon : MonoBehaviour
{
    [SerializeField] protected int pointValue;

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
}
