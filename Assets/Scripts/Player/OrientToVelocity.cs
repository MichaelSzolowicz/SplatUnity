using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientToVelocity : MonoBehaviour
{
    protected Vector3 prevPos = Vector3.zero;
    [SerializeField] protected float speed;
    [SerializeField] protected PlayerController playerController;

    protected void Start()
    {
        prevPos = transform.position;
    }

    protected void Update()
    {
        Vector3 direction = playerController.Velocity;
        Vector3 flatDirection = direction;
        flatDirection.y = 0f;   

        if (flatDirection.magnitude < .2f) return;

        direction = Vector3.Normalize(direction);

        //direction = Vector3.Lerp(transform.forward, direction, speed * Time.deltaTime);

        transform.LookAt(transform.position + direction);    

        prevPos = transform.position;
    }
}
