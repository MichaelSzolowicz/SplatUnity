using Microsoft.SqlServer.Server;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientToVelocity : MonoBehaviour
{
    [SerializeField] protected PlayerController playerController;
    Vector3 flatDirection;
    Vector3 direction;

    protected void FixedUpdate()
    {
        if(playerController.Velocity.magnitude > 1f)
        {
            direction = playerController.Velocity;
        }

        transform.LookAt(transform.position + direction.normalized);
    }
}
