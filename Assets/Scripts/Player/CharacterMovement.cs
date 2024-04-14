using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class CharacterMovement : MonoBehaviour
{
    /* Internal Physics State */
    protected Vector2 horizontalVel;
    protected float verticalVel;

    /* Internal Input Values */
    protected Vector2 accumulatedInput;
    protected float jumpTimeHeld;
    protected Vector3 controlRotation;
    public Vector3 ControlRotation { get { return controlRotation; } set { controlRotation = value; } }

    /* Internal Movement State */
    protected bool isGrounded;
    protected MoveState currMoveState;
    [Tooltip("Hit info from the most recent probe against ink.")]
    protected RaycastHit surfaceProbeHit;

    /* Public Physics Parameters */
    public float maxGroundSpeed;
    public float acceleration;
    public float braking;
    public float maxWalkableSlopeAngle;
    public float gravityScale;
    public float terminalVelocity;
    public float maxJumpHeight;
    public float maxJumpHoldTime;

    protected CapsuleCollider capsule;

    protected void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
    }

    protected void Update()
    {
        UpdatePhysics(Time.deltaTime);
    }


    /// <summary>
    /// Gather changes to physics state, like accumulated input and gravity, then apply changes.
    /// </summary>
    protected void UpdatePhysics(float deltaTime)
    {
        ApplyGravity(deltaTime);

        MoveInfo move = new MoveInfo();
        move.input = ConsumeAccumulatedInput();
        move.time = deltaTime;

        PerformMove(move);

        if(verticalVel > terminalVelocity) verticalVel = terminalVelocity;
        if(horizontalVel.magnitude > maxGroundSpeed) horizontalVel = horizontalVel.normalized * maxGroundSpeed;
        Vector3 finalVelocity = new Vector3(horizontalVel.x, verticalVel, horizontalVel.y);
        Vector3 deltaPos = finalVelocity * deltaTime;

        if(floorNormal.magnitude > .1f)
        {
            float size = deltaPos.magnitude;
            deltaPos = Vector3.ProjectOnPlane(deltaPos, floorNormal);
            deltaPos = deltaPos.normalized * size;
        }

        transform.position += deltaPos;
    }

    /// <summary>
    /// Execute a move defined in MoveInfo and apply changes to physics state.
    /// </summary>
    /// <param name="move"></param>
    protected void PerformMove(MoveInfo move)
    {
        Vector2 a = move.input * acceleration;
        if (a.magnitude > acceleration) { a = a.normalized * acceleration; }
        horizontalVel += a * move.time;
        //else horizontalVel = Vector3.zero;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(name + " collision with " + collision.collider.gameObject.name);  
        HandleCollision(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    protected void HandleCollision(Collision collision)
    {


        // Correct our position so the capsule doesn't clip inside other geometry.
        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collision.collider, collision.transform.position, collision.transform.rotation, out correction, out distance);

        if (distance > 0)
        {
            // Only apply the correction if it is valid.
            transform.Translate(correction * distance);
        }

        // How many contacts do we typically get?
        Debug.Log(name + " contact count " + collision.contactCount);   // Quite a few. Sometimes as many as four when colliding wiht two surfaces on the same mesh collider.


        foreach (var contact in collision.contacts)
        {
            // Apply normal force. (Do it on  each  one for now; later it might(?) be faster to  pick one to do it on).
            Vector2 flatNormal = new Vector2(contact.normal.x, contact.normal.z);
            float velAlongNormal = Vector2.Dot(horizontalVel, flatNormal);
            horizontalVel -= flatNormal * velAlongNormal;
            break;
        }


        // Cancel z velocity if moving down towards the ground
        if (isGrounded && verticalVel < 0) verticalVel = 0;

        UpdateIsGrounded();
    }

    Vector3 floorNormal;

    
    protected void UpdateIsGrounded()
    {
        Vector3 start = transform.position;
        Vector3 end = start + Vector3.down * ((capsule.height / 2) + .1f);
        RaycastHit hit;

        // Check if slope is walkable
        if (Physics.Linecast(start, end, out hit))
        {
            float angle = Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) * Mathf.Rad2Deg;
            isGrounded = (angle <= maxWalkableSlopeAngle);

            floorNormal = hit.normal;

            Debug.Log(name + " grounded? " + isGrounded);
        }
        else
        {
            isGrounded = false;

            floorNormal = Vector3.zero;
        }
    }
    

    protected void ApplyGravity(float deltaTime)
    {
        //if(!isGrounded) 
            verticalVel -= 9.8f * gravityScale * deltaTime;
    }

    /// <summary>
    /// Add a directional input.
    /// </summary>
    /// <param name="input"></param>
    public void AddInput(Vector2 input)
    {
        accumulatedInput += input;
    }

    /// <summary>
    /// Zero out accumulated input and return its previous value.
    /// </summary>
    /// <returns></returns>
    protected Vector2 ConsumeAccumulatedInput()
    {
        Vector2 temp = accumulatedInput;    
        accumulatedInput = Vector2.zero;
        return temp;
    }
}
