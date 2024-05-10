using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// [Player Controller]
/// [05-01-23]
/// [Szolowicz, Michael]
/// Defines actions a default player character is capable of. 
/// </summary>
/// 


public enum MovementState
{
    Walking,
    Swimming,
    WallSwimming,
    EnemyInk
}


public class PlayerController : MonoBehaviour
{
    protected const float GRAVITY_CONSTANT = 9.8f;
    protected const float STOP_MIN_THRESHOLD = .2f;

    [Header("==== Child object references ====")]
    /** Child obj references **/
    public GameObject mesh;
    public ThirdPersonCamera cameraControls;
    public CapsuleCollider capsule;
    public Animator animator;
    public SkinnedMeshRenderer meshRenderer;

    /** Internal objects **/
    protected PlayerControls playerControls;
    protected SplatmaskReader SplatmaskReader;
    protected Shooter shooter;
    public Shooter Shooter { get { return shooter; } set { shooter = value; } }

    /** Movement state control **/
    [SerializeField]
    protected Vector4 team;
    [Header("==== TESTONLY exposd for testing purposes, do not edit. ====")]
    [SerializeField]
    protected MovementState currentMovementState;
    [SerializeField]
    protected bool isSquid;
    protected RaycastHit surfaceProbeHit;

    /** Input variables **/
    [Tooltip("Variables with Input in their name are clamped to max speed accel & speed in UpdatePhysics. Generally used for horizontal movement.")]
    protected Vector3 pendingInput, pendingInputForce, inputVelocity, pendingVerticalInput;
    [Tooltip("Vertical input variables are added after input has been clamped, and are not clamped themselves. Keeps gravity consistent even when over the max speed.")]
    protected Vector3 verticalVelocity;
    [Tooltip("Normal of the most recently handled collision. Zeroed when not colliding.")]
    protected Vector3 groundNormal;
    [SerializeField, Tooltip("True if the character is colliding.")]
    protected bool grounded;
    protected Vector3 pendingJumpForce;

    public Vector3 Velocity { get { return prevDelta.normalized * (inputVelocity + verticalVelocity).magnitude; } }

    /** Adjustable properties **/
    [Header("Movement"), SerializeField]
    protected float maxAcceleration = 1.0f;
    protected float baseMaxAcceleration;
    [SerializeField]
    protected float maxHorizontalSpeed = 5.0f;
    protected float baseMaxHorizontalSpeed;
    [SerializeField, Tooltip("Magnitude of force to apply on input.")]
    protected float inputStrength = 1.0f;
    [SerializeField, Tooltip("Constant force applied opposing input velocity.")]
    protected float braking = 0.0f;
    [SerializeField]
    protected float wallClimbStrength;
    [SerializeField]
    protected float ledgeReducedAcceleration = 10.0f;
    [SerializeField]
    protected float jumpForce = 10;

    [Header("Gravity"), SerializeField]
    protected float gravityScale = 1.0f;
    protected float defaultGravityScale;

    [Header("Slopes"), SerializeField]
    protected float minSlopeGradation = .0f;
    [SerializeField]
    protected float slopeCheckTolerance = .01f;

    [Header("Ink Detection"), SerializeField]
    protected float inkAlphaMinThreshold;
    [SerializeField]
    protected float updateMovementStateDelay = 0.032f;
    [SerializeField]
    protected float enemyInkSlowingFactor;
    [SerializeField]
    protected float swimSpeedMultiplier = 2f;
    [SerializeField]
    protected float downwardProbeDist = .4f;

    [Header("Transition Effects")]
    [SerializeField] protected GameObject kidForm;
    [SerializeField] protected GameObject octoForm;
    [SerializeField] protected GameObject toSquidParticle;
    [SerializeField] protected float squidTransitionDuration;

    protected IEnumerator toSquidCoroutine;

    protected void Awake()
    {
        //Controls setup
        playerControls = new PlayerControls();
        playerControls.Enable();
        playerControls.Walking.Squid.performed += EnterSquid;
        playerControls.Walking.Squid.canceled += ExitSquid;
        playerControls.Walking.Shoot.performed += OnPressedShoot;
        playerControls.Walking.Shoot.canceled += OnReleaseShoot;
        playerControls.Walking.Jump.performed += Jump;

        //Default values
        defaultGravityScale = gravityScale;
        baseMaxHorizontalSpeed = maxHorizontalSpeed;
        baseMaxAcceleration = maxAcceleration;
        currentMovementState = MovementState.Walking;
        isSquid = false;

        // Object initialization
        SplatmaskReader = gameObject.AddComponent<SplatmaskReader>();

        /// Important: Start movement state update loop.
        Invoke("UpdateMovementState", .032f);
    }

    protected void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        prevPos = transform.position;
    }

    protected void Update()
    {
        /// Camera rotation
        Vector2 mouseDelta = playerControls.Walking.Camera.ReadValue<Vector2>() * Time.timeScale;
        cameraControls.CameraUpdate(mouseDelta.x, mouseDelta.y);
        mesh.transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, cameraControls.yRotation, transform.rotation.eulerAngles.z);
        shooter.transform.rotation = Quaternion.LookRotation(cameraControls.cameraTransform.forward);

        /// Anim
        UpdateAnimation();
    }

    protected void FixedUpdate()
    {
        UpdatePhysics();
    }

    /// <summary>
    /// Probes the surface, looking for a SplatableObject to read ink values from.
    /// This function gets the values and calls ReadPixel() on SplatmaskReader, which in turn calls 
    /// FinishUpdateMovementState() once it finished reading the pixel value.
    /// </summary>
    protected void UpdateMovementState()
    {
        SplatableObject splatObj;
        RaycastHit hit;
        Vector3 origin = capsule.transform.TransformPoint(capsule.center);

        /// Forward probe
        Ray ray = new Ray(origin, mesh.transform.forward);
        bool isValidHit = Physics.Raycast(ray, out hit, capsule.radius + .1f);
        surfaceProbeHit = hit;

        /** TESTONLY **/ 
        Debug.DrawRay(ray.origin, ray.direction, Color.blue, .1f);
        /** ENDTEST **/

        if(isValidHit && currentMovementState != MovementState.EnemyInk)
        {
            splatObj = hit.collider.GetComponent<SplatableObject>();
            if(splatObj)
            {
                SplatmaskReader.ReadPixel(splatObj.Splatmask, hit.textureCoord, FinishUpdateMovementSate);
                return;
            }
        }

        /// Downward probe.
        ray = new Ray(origin, Vector3.down);
        isValidHit = Physics.Raycast(ray, out hit, capsule.height + capsule.radius + downwardProbeDist);
        surfaceProbeHit = hit;

        /** TESTONLY **/
        Debug.DrawRay(ray.origin, ray.direction * (capsule.height + capsule.radius + downwardProbeDist), Color.red, .1f);
        /** ENDTEST **/

        if (isValidHit)
        {
            splatObj = hit.collider.GetComponent<SplatableObject>();
            if (splatObj)
            {
                SplatmaskReader.ReadPixel(splatObj.Splatmask, hit.textureCoord, FinishUpdateMovementSate);
            }
        }
        else FinishUpdateMovementSate(Color.clear);
    }

    /// <summary>
    /// Updates the movement state using color as the ink color and surfaceProbeHit as the surface.
    /// Invokes UpdateMovementState() after finishing, to maintain the update movement state loop. To
    /// cancel the update movement state loop, call CancelInvoke().
    /// </summary>
    /// <param name="color"></param>
    protected void FinishUpdateMovementSate(Color color)
    {
        // This function can be called late by SplatmaskReader, so make sure this still exists before doing anything.
        if (!this) return;

        /// Wall Swimming
        if(surfaceProbeHit.collider && 
            Vector3.Dot(surfaceProbeHit.normal, Vector3.up) - slopeCheckTolerance < minSlopeGradation
            && color.a > inkAlphaMinThreshold && color.r > color.g && isSquid)
        {
            //print("Wall Swimming");
            currentMovementState = MovementState.WallSwimming;
            maxHorizontalSpeed =  baseMaxHorizontalSpeed;
            grounded = false;

            octoForm.SetActive(false);
            kidForm.SetActive(false);
            //meshRenderer.enabled = false;

            Invoke("UpdateMovementState", updateMovementStateDelay);
            return;
        }


        /// Enemy ink
        if (color.a > inkAlphaMinThreshold && color.g > color.r)
        {
            if (currentMovementState == MovementState.EnemyInk)
            {
                maxHorizontalSpeed = baseMaxHorizontalSpeed * enemyInkSlowingFactor; // Mathf.SmoothStep(maxHorizontalSpeed, baseMaxHorizontalSpeed * enemyInkSlowingFactor, smoothMaxSpeedDecrease);
                Invoke("UpdateMovementState", updateMovementStateDelay);
                return;
            }
            //print("EnemyInk");
            currentMovementState = MovementState.EnemyInk;
            isSquid = false;

            octoForm.SetActive(false);
            kidForm.SetActive(true);
            //meshRenderer.enabled = true;

            Invoke("UpdateMovementState", updateMovementStateDelay);
            return;
        }

        /// Swimming
        if (color.a > inkAlphaMinThreshold && color.r > color.g && isSquid && grounded)
        {
            if(currentMovementState == MovementState.Swimming)
            {
                maxHorizontalSpeed = baseMaxHorizontalSpeed * swimSpeedMultiplier;
                Invoke("UpdateMovementState", updateMovementStateDelay);
                return;
            }
            print("Swimming");
            currentMovementState = MovementState.Swimming;
            if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());

            octoForm.SetActive(false);
            kidForm.SetActive(false);
            //meshRenderer.enabled = false;

            Invoke("UpdateMovementState", updateMovementStateDelay);
            return;
        }

        /// Default case
        //print("Walking");
        currentMovementState = MovementState.Walking;
        maxHorizontalSpeed = baseMaxHorizontalSpeed;

        if (playerControls.Walking.Squid.IsPressed() && !isSquid) EnterSquid(new InputAction.CallbackContext());

        octoForm.SetActive(isSquid);
        kidForm.SetActive(!isSquid);
        //meshRenderer.enabled = !isSquid;

        Invoke("UpdateMovementState", updateMovementStateDelay);
        return;
    }

    /// <summary>
    /// Update the value of pending input, accounting for current movement state.
    /// </summary>
    /// <returns></returns>
    private void GetInput() 
    {
        pendingInput = new Vector3(playerControls.Walking.MovementInput.ReadValue<Vector2>().x, 0, playerControls.Walking.MovementInput.ReadValue<Vector2>().y);

        /// If we are wall swimming
        if (isSquid && currentMovementState == MovementState.WallSwimming)
        {
            grounded = false;   // Wall swimming does not count as a grounded state.

            // Not a big fan of vertical input being a vector 3. May change it to just a float for the y value.
            pendingVerticalInput = Quaternion.LookRotation(-surfaceProbeHit.normal, transform.up) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
            pendingVerticalInput.x = 0; pendingVerticalInput.z = 0;

            pendingInput = Quaternion.LookRotation(-surfaceProbeHit.normal, transform.up) * Quaternion.Euler(-90f, 0, 0) * pendingInput;
            pendingInput.y = 0;
        }
        /// Else, if we midair
        else if (isSquid && !surfaceProbeHit.collider && !grounded)
        {
            grounded = false;
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;

            /// Reduce our horizontal accel slightly so we fly straight up a bit before coming down and landing on the ledge.
            maxAcceleration = ledgeReducedAcceleration;
        }
        /// Else if we are falling downwards.
        else if(isSquid && !surfaceProbeHit.collider && verticalVelocity.y < 0f)
        {
            maxAcceleration = baseMaxAcceleration;
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;
        }
        /// Else default case
        else
        {
            //print("else");
            maxAcceleration = baseMaxAcceleration;
            gravityScale = defaultGravityScale;
            pendingInput = Quaternion.LookRotation(mesh.transform.forward, Vector3.up) * pendingInput;
        }

        Debug.DrawLine(transform.position, transform.position + pendingInput * 5, Color.red, .1f); 
    }

    /// <summary>
    /// Updates physics variables and move character.
    /// </summary>
    protected void UpdatePhysics() 
    {
        GetInput();
        AddInputForce(pendingInput * inputStrength);
        AddFriction();

        // Calc acceleration, Ignoring mass.
        Vector3 a = pendingInputForce;
        if (a.magnitude >= maxAcceleration) a = a.normalized * maxAcceleration;
      
        inputVelocity += a * Time.fixedDeltaTime;
        if (inputVelocity.magnitude > maxHorizontalSpeed) inputVelocity = inputVelocity.normalized * maxHorizontalSpeed;

        Vector3 delta = inputVelocity * Time.fixedDeltaTime;

        // Maintain velocity parallel to floor
        if (Vector3.Dot(groundNormal, Vector3.up) - slopeCheckTolerance > minSlopeGradation && grounded)
        {
            float magnitude = delta.magnitude;
            delta = Vector3.ProjectOnPlane(delta, groundNormal);
            delta = delta.normalized * magnitude;
        }

        // Keep gravity calculation seperate. If they get normalized when the character goes overspeed, horizontally moving charcters will fall more slowly.
        if (!grounded)
        {
            verticalVelocity += Vector3.down * GRAVITY_CONSTANT * gravityScale * Time.fixedDeltaTime;

            delta += verticalVelocity * Time.fixedDeltaTime;
        }
        else
        {
            verticalVelocity += pendingJumpForce * Time.fixedDeltaTime;
            delta += verticalVelocity * Time.fixedDeltaTime;
            pendingJumpForce = Vector3.zero;
        }

        transform.position += delta;
        pendingInputForce = Vector3.zero;

        prevDelta = delta;

        //Debug.Log(name + " movement state = " + currentMovementState.ToString());
    }

    Vector3 prevDelta = Vector3.zero;

    /// <summary>
    /// Adds the provided force, and the internal pendingVerticalInput to pendingInputForce and pendingVerticalInput respectively.
    /// Yes, this is very confusing. I wrote this while very tired and will fix it down the road if I decide to come back to this project.
    /// </summary>
    /// <param name="force"></param>
    public void AddInputForce(Vector3 force)
    {
        verticalVelocity += pendingVerticalInput * Time.fixedDeltaTime * wallClimbStrength;
        pendingVerticalInput = Vector3.zero;

        pendingInputForce += force;
    }

    protected void OnCollisionExit(Collision collision)
    {
        grounded = false;
        groundNormal = Vector3.zero;
    }

    protected void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    protected void OnCollisionStay(Collision collision)
    {
        HandleCollision(collision);
    }

    /// <summary>
    /// Update collision related globals and correct our position.
    /// NOTE velocity is not negated when hitting a wall, meaning in certain cases the player can appear to get
    /// "stuck" to a wall as they overcome their opposing velocity. This project has high braking and acceleration
    /// values, meaning this isn't going to crop up at this time.
    /// </summary>
    /// <param name="collision"></param>
    protected void HandleCollision(Collision collision)
    {
        // Check if the slope is walkable
        foreach(ContactPoint contactPoint in collision.contacts)
        {
            if (Vector3.Dot(contactPoint.normal, Vector3.up) - slopeCheckTolerance > minSlopeGradation)
            {
                grounded = true;
                verticalVelocity.y = 0f;
                break;
            }
            else
            {
                grounded = false;
            }
        }


        groundNormal = collision.GetContact(0).normal;

        Debug.Log(name + " Ground Normal: " + groundNormal);
        Debug.Log(name + " comparision dot: " + (Vector3.Dot(collision.GetContact(0).normal, Vector3.up) - slopeCheckTolerance));

        // Correct our position wo the capsule doesn't clip inside other geometry.
        Vector3 correction;
        float distance;
        Physics.ComputePenetration(GetComponent<CapsuleCollider>(), transform.position, transform.rotation, collision.collider, collision.transform.position, collision.transform.rotation, out correction, out distance);

        if (distance > 0)
        {
            // Only apply the correction if it is valid.
            transform.Translate(correction * distance);
        }
    }

    /// <summary>
    /// Going to keep this as a simple braking factor.
    /// Strong enough braking also keeps us from maintaining inputVelocity while running into walls.
    /// </summary>
    /// <param name="collision"></param>
    protected void AddFriction()
    {
        pendingInputForce += braking * -inputVelocity.normalized;
    }

    protected void EnterSquid(InputAction.CallbackContext context)
    {
        print("Enter Squid");

        // Can't swim in enemy ink
        if (currentMovementState == MovementState.EnemyInk) return;

        if (!context.performed) return;

        animator.SetBool("isSquid", true);

        // TODO: Setting to squid coroutine variable every time I call it seems unnecessary.
        if (toSquidCoroutine != null)
            StopCoroutine(toSquidCoroutine);
        toSquidCoroutine = ToSquidCoroutine();
        StartCoroutine(toSquidCoroutine);   
    }

    protected IEnumerator ToSquidCoroutine()
    {
        Debug.Log("To Squid Coroutine");

        yield return new WaitForSeconds(squidTransitionDuration);

        isSquid = true;

        octoForm.transform.LookAt(transform.position + Quaternion.AngleAxis(cameraControls.yRotation, Vector3.up) * Vector3.forward);

        kidForm.SetActive(false);
        //meshRenderer.enabled = false;
        octoForm.SetActive(true);

        toSquidParticle.SetActive(false);
        toSquidParticle.SetActive(true);
    }

    protected void ExitSquid(InputAction.CallbackContext context)
    {
        print("Exit Squid");

        animator.SetBool("isSquid", false);

        isSquid = false;

        kidForm.SetActive(true);
        //meshRenderer.enabled = true;
        octoForm.SetActive(false);

        currentMovementState = MovementState.Walking;
        maxHorizontalSpeed = baseMaxHorizontalSpeed;

        StopCoroutine(toSquidCoroutine);
    }

    protected void OnPressedShoot(InputAction.CallbackContext context)
    {
        print("start shooting");
        shooter.StartShooting();
    }

    protected void OnReleaseShoot(InputAction.CallbackContext context)
    {
        print("stop shooting");
        shooter.StopShooting();
    }

    protected void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(name + " jump.");
        pendingJumpForce = (Vector3.up * jumpForce);
    }

    Vector3 prevPos;

    protected void UpdateAnimation()
    {
        Vector3 dp = transform.position - prevPos;
        dp = (dp / Time.fixedDeltaTime);
        prevPos = transform.position;

        dp.x = Vector3.Dot(inputVelocity, mesh.transform.right) / maxHorizontalSpeed;
        if (Mathf.Abs(dp.x) < .25f) dp.x = 0;
        dp.z = Vector3.Dot(inputVelocity, mesh.transform.forward) / maxHorizontalSpeed;
        if(Mathf.Abs(dp.z) < .25f) dp.z = 0;


        //Debug.Log(name + " vel " + dp + " speed " + inputVelocity.magnitude);
        animator.SetFloat("speedX", dp.x);
        animator.SetFloat("speedY", dp.z);
        //animator.SetBool("isSquid", currentMovementState == MovementState.Swimming || currentMovementState == MovementState.WallSwimming);
    }
}   
