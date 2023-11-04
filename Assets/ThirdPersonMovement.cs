using System.Collections;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    [SerializeField] bool debugMode = false;

    [Header("Scene References")]
    [SerializeField] CharacterController controller;
    [SerializeField] Transform cam;
    [SerializeField] Animator animator;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundMask;

    [Header("Player Base Stats")]
    [SerializeField] float walkSpeed = 10f;
    [SerializeField] float runSpeed = 15f;
    [SerializeField] float gravity = -9f;
    [SerializeField] float turnSmoothTime = 0.1f;
    //[SerializeField] float jumpPower = 5f;

    [Header("Environmental Stats")]
    [SerializeField] float distanceToGround = .4f;
    [Header("Pickup and Drop")]
    [SerializeField] Transform grabPoint;
    [SerializeField] float pickupRange = 2f;
    private Interactable currentHeldItem;


    float turnSmoothVelocity;
    float speed;
    float idleAnimation = 0f;

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
        //CheckJump();
        HandleGravity();

        //if (lastState != currentState) { HandleAnimation(); }
        //animator.SetBool("airborne", !isGrounded);

        if (isIdle)
        {
            if (idleAnimation < 1f)
            {
                idleAnimation += .1f * Time.deltaTime;
            }
            else { idleAnimation = 0f; }
            animator.SetFloat("idleAnimation", idleAnimation);
        }

        animator.SetFloat("forwardSpeed", speed / runSpeed);

        //print(currentState);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (grabPoint.childCount > 0)
            {
                DropItem();
            }
            else
            {
                AttemptPickup();
            }
        }

    }

    //private void ApplyGravity()
    //{
    //    if (controller.isGrounded && verticalVelocity.y < 0)
    //    {
    //        verticalVelocity.y = -2f; // Some small value to ensure character is grounded
    //    }
    //    else
    //    {
    //        verticalVelocity.y += gravity * Time.deltaTime;
    //    }

    //    controller.Move(verticalVelocity * Time.deltaTime); // Apply gravity movement
    //}


    //private IEnumerator IdleAnimations()
    //{
    //    animator.SetFloat("idleAnimation", idleAnimation);
    //    if (idleAnimation < 1f)
    //    { idleAnimation = 1f; }
    //    else { idleAnimation = 0f; }
    //    yield return new WaitForSeconds(1.5f);
    //}

    private void HandleAnimation()
    {
        //switch (currentState)
        //{
        //    //case State.Idle:
        //    //    animator.SetTrigger("idle");
        //    //    break;
        //    //case State.Walking:
        //    //    animator.SetTrigger("walking");
        //    //    break;
        //    //case State.Running:
        //    //    animator.SetTrigger("running");
        //    //    break;
        //    case State.Airborne:
        //        break;
        //    default:
        //        break;
        //}
    }

    //private void CheckJump()
    //{
    //    if (isGrounded && Input.GetButtonDown("Jump"))
    //    {
    //        animator.SetTrigger("jump");
    //        print("Wait For Jump");
    //        StartCoroutine(WaitForJump());
    //    }
    //}

    //private IEnumerator WaitForJump()
    //{
    //    yield return new WaitForSeconds(.1f);
    //    velocity.y = Mathf.Sqrt(jumpPower * -2 * gravity);
    //    print("Jump");

    //    yield return new WaitForSeconds(.3f);
    //    animator.SetTrigger("jump");
    //    print("Airborne");
    //}

    private void HandleGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
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

        isIdle = direction.magnitude < 0.1f;

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
        if (isGrounded && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) && !isIdle))
        {
            currentState = State.Running;
            speed = runSpeed;
        }
        else if (isGrounded && !isIdle)
        {
            speed = walkSpeed;
        }
    }

    private void AttemptPickup()
    {
        if (currentHeldItem != null) return; // If already holding something, do nothing

        Collider[] itemsInRange = Physics.OverlapSphere(transform.position, pickupRange);
        foreach (Collider item in itemsInRange)
        {
            Interactable interactable = item.GetComponent<Interactable>();
            if (interactable)
            {
                currentHeldItem = interactable;
                interactable.PickUp(grabPoint);
                break;
            }
        }
    }

    private void DropItem()
    {
        currentHeldItem.Drop();
        currentHeldItem = null;
    }

}
