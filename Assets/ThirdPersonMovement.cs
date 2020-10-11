using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    [SerializeField] bool debugMode = false;

    [Header("Scene References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cam;
    [SerializeField] CinemachineFreeLook freeLookCam;
    [SerializeField] Animator animator;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Player Base Stats")]
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float runSpeed = 15f;
    [SerializeField] float gravity = -9f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float jumpPower = 5f;

    [Header("Environmental Stats")]
    [SerializeField] float distanceToGround = .4f;


    float turnSmoothVelocity;
    float speed = 10f;
    bool isGrounded;
    bool isIdle;
    Vector3 velocity;
    Vector3 currentSpeed;

    enum State
    {
        Idle,
        Walking,
        Running,
        Airborne
    }

    State currentState;

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR

        if (debugMode)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, distanceToGround);
        }

#endif
    }

    private void Start()
    {
        currentState = State.Idle;
        speed = 0f;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        State lastState = currentState;
        HandleBaseMovement();
        CheckSprint();
        CheckJump();
        HandleGravity();
        if (lastState != currentState) { HandleAnimation(); }

        animator.SetFloat("forwardSpeed", speed / runSpeed);
        //print(currentState);
    }

    private void HandleAnimation()
    {
        //switch (currentState)
        //{
        //    case State.Idle:
        //        animator.SetTrigger("idle");
        //        break;
        //    case State.Walking:
        //        animator.SetTrigger("walking");
        //        break;
        //    case State.Running:
        //        animator.SetTrigger("running");
        //        break;
        //    case State.Airborne:
        //        animator.SetTrigger("airborne");
        //        break;
        //    default:
        //        break;
        //}
    }

    private void CheckJump()
    {
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            print("Jump");
            velocity.y = Mathf.Sqrt(jumpPower * -2 * gravity);
        }
    }

    private void HandleGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
            return;
        }
        if (!isGrounded)
        {
            currentState = State.Airborne;
            velocity.y += gravity * Time.deltaTime;
        }
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleBaseMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, distanceToGround, groundMask);
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        isIdle = direction.magnitude >= 0.1f;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);

            currentState = State.Walking;

            //freeLookCam.m_XAxis.Value = transform.rotation.y;
            //print(transform.rotation.y);
        }
        else { currentState = State.Idle; speed = 0f; }

    }

    private void CheckSprint()
    {
        if (isGrounded && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && isIdle))
        {
            currentState = State.Running;
            speed = runSpeed;
        }
        else if (isGrounded && isIdle)
        {
            speed = walkSpeed;
        }
    }
}
