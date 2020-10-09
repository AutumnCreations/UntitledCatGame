using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    [SerializeField] bool debugMode = false;

    [Header("Scene References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cam;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Player Base Stats")]
    [SerializeField] float speed = 10f;
    [SerializeField] float gravity = -9f;
    [SerializeField] float turnSmoothTime = 0.1f;
    [SerializeField] float jumpPower = 5f;

    [Header("Environmental Stats")]
    [SerializeField] float distanceToGround = .4f;


    float turnSmoothVelocity;
    bool isGrounded;
    Vector3 velocity;

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
        //distanceToGround = GetComponent<Collider>().bounds.extents.y;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, distanceToGround, groundMask);

        CheckSprint();
        HandleBaseMovement();
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            print("Jump");
            velocity.y = Mathf.Sqrt(jumpPower * -2 * gravity);
        }

        HandleGravity();

        print(isGrounded);
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -.5f;
            return;
        }

        velocity.y += gravity * Time.deltaTime;
    }

    private void HandleBaseMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }
    }

    private void CheckSprint()
    {
        if (isGrounded && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
        {
            speed = 15f;
        }
        else
        {
            speed = 10f;
        }
    }
}
