using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Inker for rigidbody collisions.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CollisionInker : Inker
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision)
    {
        SplatableObject splatObj = collision.collider.GetComponent<SplatableObject>();
        if(splatObj)
        {
            //Debug.Log(name + " hit " + collision.gameObject.name);
            //Debug.DrawLine(collision.contacts[0].point, collision.contacts[0].point + collision.contacts[0].normal * 3, Color.magenta, 0.1f);

            splatObj.DrawSplat(collision.contacts[0].point, collision.contacts[0].normal, radius, hardness, strength, inkColor);
        }
    }
}
