using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    protected Rigidbody rb;

    protected void Start()
    {
        capsule = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
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

        transform.position += deltaPos;

        // Sweep against new pos
        RaycastHit[] hits = new RaycastHit[5];
        hits = Physics.CapsuleCastAll(transform.position + Vector3.up * .5f, transform.position - Vector3.up * .5f, .5f, deltaPos.normalized, deltaPos.magnitude);

        if (hits.Length <= 0) isGrounded = false;
        foreach (RaycastHit hit in hits)
        {
            bool isGroundedSet = false;
            Vector3 normal = CorrectOverlap(hit);

            if(normal.magnitude > .01f)
            {
                normal = normal.normalized;
                Debug.DrawLine(hit.point, hit.point + normal * 3, Color.magenta, 10f);

                Vector2 flatNormal = new Vector2(normal.x, normal.z);
                float velAlongNormal = Vector3.Dot(horizontalVel, flatNormal);
                horizontalVel -= flatNormal * velAlongNormal;

                Debug.Log(" impulse " + (flatNormal * velAlongNormal));

                float angle = Mathf.Acos(Vector3.Dot(normal, Vector3.up)) * Mathf.Rad2Deg;
                Debug.Log("angle " + (angle < maxWalkableSlopeAngle));
                if(!isGroundedSet) isGroundedSet = isGrounded = angle < maxWalkableSlopeAngle;
            }
        }

        //UpdateIsGrounded();
        //Debug.Log(isGrounded);
        if (isGrounded) verticalVel = 0;


        // Apply remainder of delta pos after correction parallel to surface
    }

    protected Vector3 CorrectOverlap(RaycastHit hit)
    {
        Collider collider = hit.collider;
        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collider, collider.transform.position, collider.transform.rotation, out correction, out distance);
        transform.position += correction * distance;
        return correction * distance;
    }

    protected void UpdateIsGrounded()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position - Vector3.up * ((capsule.height / 2) + .3f);
        RaycastHit hit;

        if (Physics.Linecast(start, end, out hit))
        {
            float angle = Mathf.Acos(Vector3.Dot(hit.normal, Vector3.up)) * Mathf.Rad2Deg;
            Debug.Log(angle);
            isGrounded = angle < maxWalkableSlopeAngle;
        }
        else
        {
            isGrounded = false;
        }
    }

    /// <summary>
    /// Execute a move defined in MoveInfo and apply changes to physics state.
    /// </summary>
    /// <param name="move"></param>
    protected void PerformMove(MoveInfo move)
    {
        //Debug.Log(isGrounded);
        Vector2 a = move.input * acceleration;
        if (a.magnitude > acceleration) { a = a.normalized * acceleration; }
        if(isGrounded) horizontalVel += a * move.time;
    }
    

    protected void ApplyGravity(float deltaTime)
    {
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
