
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

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
    protected Vector3 groundNormal;

    /* Public Physics Parameters */
    public float maxGroundSpeed;
    public float acceleration;
    public float braking;
    public float maxWalkableSlopeAngle;
    public float gravityScale;
    public float terminalVelocity;
    public float maxJumpHeight;
    public float maxJumpHoldTime;

    /* Components */
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
        // Update internal physics state
        ApplyGravity(deltaTime);
        UpdateGroundNormal();

        // Update input based physics state
        MoveInfo move = new MoveInfo(); // Construct the move
        move.input = ConsumeAccumulatedInput();
        move.time = deltaTime;

        PerformMove(move);  // Apply the move to physics state

        // Clamp speed
        if(Mathf.Abs(verticalVel) > terminalVelocity) verticalVel = terminalVelocity * Mathf.Sign(verticalVel);
        if(horizontalVel.magnitude > maxGroundSpeed) horizontalVel = horizontalVel.normalized * maxGroundSpeed;

        // Construct final delta position
        Vector3 finalVelocity = new Vector3(horizontalVel.x, 0, horizontalVel.y);

        if (isGrounded)
        {
            // Maintain velocity parallel to floor.
            float speed = finalVelocity.magnitude;
            finalVelocity = Vector3.ProjectOnPlane(finalVelocity, groundNormal);
            finalVelocity = finalVelocity.normalized * speed;
        }

        finalVelocity.y += verticalVel; // Add in gravity

        // Actual delta position
        Vector3 deltaPos = finalVelocity * deltaTime;
        if (isGrounded) deltaPos.y -= .01f; // Stick to ground

        // Sweep the move
        RaycastHit[] hits = new RaycastHit[5];
        hits = Physics.CapsuleCastAll(transform.position + Vector3.up * .5f, transform.position - Vector3.up * .5f, .5f, deltaPos.normalized, deltaPos.magnitude);

        // Update position and handle overlaps
        transform.Translate(deltaPos);
        isGrounded = false; // Assume not grounded; depentration will determine the correct value.
        foreach (RaycastHit hit in hits)
        {
            Vector3 depenetration = CorrectOverlap(hit);

            depenetration = depenetration.normalized;

            // Is wakable?
            float angle = Mathf.Acos(Vector3.Dot(depenetration, Vector3.up)) * Mathf.Rad2Deg;
            if (angle < maxWalkableSlopeAngle)
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                }
            }
            else
            {
                // Impulse
                Vector2 flatNormal = new Vector2(depenetration.x, depenetration.z);
                float velAlongNormal = Vector3.Dot(horizontalVel, flatNormal);
                horizontalVel -= flatNormal * velAlongNormal;
            }
        }

        // Naive floor "normal impulse"
        if (isGrounded) verticalVel = 0;

        Debug.Log(isGrounded);
    }

    /// <summary>
    /// Try to depenetrate character from collider in hit.
    /// </summary>
    /// <param name="hit"></param>
    /// <returns>The scaled depentration vector.</returns>
    protected Vector3 CorrectOverlap(RaycastHit hit)
    {
        Collider collider = hit.collider;
        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collider, collider.transform.position, collider.transform.rotation, out correction, out distance);
        transform.position += correction * distance;
        return correction * distance;
    }

    /// <summary>
    /// Raycast against the ground to update groundNormal.
    /// Useful for getting the slope.
    /// </summary>
    protected void UpdateGroundNormal()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position - Vector3.up * ((capsule.height / 2) + 1.1f);
        RaycastHit hit;

        if (Physics.Linecast(start, end, out hit))
        {
            groundNormal = hit.normal;
        }
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
    }
    
    /// <summary>
    /// Add downward acceleration equal to -9.8 * gravityScale * deltaTime
    /// </summary>
    /// <param name="deltaTime"></param>
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
