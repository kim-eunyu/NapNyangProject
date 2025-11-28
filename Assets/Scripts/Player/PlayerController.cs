using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    public float rotationSpeed = 15f;
    public Animator animator;
    public Camera cam; 

    private Rigidbody rb;
    private Vector3 moveInput; 
    private Vector3 worldMoveDirection; 
    private bool isRunning;
    private bool isGrounded = true;
    private PlayerMental playerMental; 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMental = GetComponent<PlayerMental>(); 

        rb.freezeRotation = true;
        rb.linearDamping = 5f; 
    }

    void Update()
    {
        HandleMovementInput();
        CalculateWorldMoveDirection(); 
        HandleActions();
        
        // [순서 중요!] 땅 체크를 먼저 하고 애니메이션을 처리해야 정확해용
        CheckGroundWithRaycast(); 
        HandleAnimations(); 
    }

    void FixedUpdate()
    {
        HandleMovement();
        HandleRotation(); 
    }

    void HandleMovementInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        moveInput = new Vector3(h, 0, v); 

        if (IsTired())
        {
            isRunning = false;
        }
        else
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        }
    }

    void CalculateWorldMoveDirection()
    {
        if (moveInput.magnitude < 0.1f)
        {
            worldMoveDirection = Vector3.zero;
            return;
        }
        
        if (cam == null)
        {
            worldMoveDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;
            return;
        }

        Vector3 camForward = cam.transform.forward;
        Vector3 camRight = cam.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        worldMoveDirection = (camForward * moveInput.z + camRight * moveInput.x).normalized;
    }

    void HandleMovement()
    {
        if (worldMoveDirection.magnitude > 0.1f)
        {
            float moveSpeed = isRunning ? runSpeed : walkSpeed;
            
            if (IsTired())
            {
                moveSpeed = walkSpeed * 0.5f; 
            }

            Vector3 targetVelocity = worldMoveDirection * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; 

            rb.linearVelocity = targetVelocity;
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void HandleRotation()
    {
        if (worldMoveDirection.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(worldMoveDirection);

            float currentRotationSpeed = IsTired() ? rotationSpeed * 0.7f : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                 currentRotationSpeed * Time.fixedDeltaTime);
        }
    }

    void CheckGroundWithRaycast()
    {
        float rayDistance = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        RaycastHit hit;
        bool wasGrounded = isGrounded; 
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance);

        if (wasGrounded != isGrounded)
        {
            // Debug.Log(isGrounded ? "지면 감지!" : "공중으로!");
        }

        Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
    }

    void HandleActions()
    {
        if (animator == null) return;

        if (IsTired())
        {
            animator.SetBool("IsGrooming", Input.GetKey(KeyCode.Q));
            return;
        }

        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Attack");
        if (Input.GetMouseButtonDown(1))
            animator.SetTrigger("Skill");
        if (Input.GetKeyDown(KeyCode.R))
            animator.SetTrigger("Ultimate");

        animator.SetBool("IsHiding", Input.GetKey(KeyCode.LeftControl));
        animator.SetBool("IsGrooming", Input.GetKey(KeyCode.Q));
    }

    // ▼▼▼ 여기가 으뉴님이 원하시는 대로 고쳐진 핵심입니다! ▼▼▼
    void HandleAnimations()
    {
        if (animator == null) return;

        float speedVal = 0f;

        if (moveInput.magnitude > 0.1f)
        {
            if (IsTired())
            {
                speedVal = 0.5f;
            }
            else
            {
                speedVal = isRunning ? 2f : 1f;
            }
        }

        animator.SetFloat("Speed", speedVal);
        animator.SetBool("IsTired", IsTired());

        // [수정 포인트 1] 땅에 닿아있으면 점프 애니메이션을 끕니다. (착지)
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }

        // [수정 포인트 2] "공중에 있다고" 무조건 점프 모션을 켜는 코드를 삭제했습니다.
        // 대신, 스페이스바를 눌렀을 때만 점프 모션을 켭니다!
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            // 물리적인 점프
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            
            // 애니메이션 점프 (여기서 켜짐!)
            animator.SetBool("IsJumping", true);
        }
    }
    // ▲▲▲ 수정 끝 ▲▲▲

    bool IsTired()
    {
        if (playerMental == null) return false;
        return playerMental.IsTired();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Debug.Log("Collision 감지: 착지");
        }
    }

    void OnDrawGizmos()
    {
        if (moveInput.magnitude > 0.1f) 
        {
            Gizmos.color = IsTired() ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, worldMoveDirection * 2f);
        }
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);

        Gizmos.color = isGrounded ? Color.blue : Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayOrigin, Vector3.down * 0.3f);
    }
}