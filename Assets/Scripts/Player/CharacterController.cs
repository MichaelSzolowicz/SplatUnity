using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterMovement))]
public class CharacterController : MonoBehaviour
{
    protected Quaternion controlRotation;
    public Vector3 ControlRotation { get { return controlRotation.eulerAngles; } }

    protected CharacterMovement moveComp;
    protected PlayerControls controls;

    protected void Awake()
    {
        moveComp = GetComponent<CharacterMovement>();

        controls = new PlayerControls();
        controls.Enable();


    }

    protected void Update()
    {
        Move();
    }

    protected void Move()
    {
        Vector2 input = controls.Walking.MovementInput.ReadValue<Vector2>();
        moveComp.AddInput(input);
    }
}
