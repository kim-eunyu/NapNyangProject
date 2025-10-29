using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 5f;
    public float rotationSpeed = 15f;
    public Animator animator;
    public Camera cam; // <--- 이 카메라가 꼭 할당되어야 합니다!

    private Rigidbody rb;
    private Vector3 moveInput; // (h, v) 입력 값을 저장
    private Vector3 worldMoveDirection; // (실제 이동할 월드 방향)
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
        HandleAnimations();
        CheckGroundWithRaycast(); // <--- 이 함수 내부가 수정되었습니다.
       
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

    // ▼▼▼ 여기가 수정된 부분입니다 ▼▼▼
    void CheckGroundWithRaycast()
    {
        // (기존 코드와 동일)
        float rayDistance = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        RaycastHit hit; // <--- 수정! (hit 변수 선언 추가)
        bool wasGrounded = isGrounded; // <--- 수정! (wasGrounded 로직 복구)
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance);

        // <--- 수정! (지면 상태 변화 로그 복구)
        if (wasGrounded != isGrounded)
        {
            Debug.Log(isGrounded ? "지면 감지!" : "공중으로!");
        }

        // 디버그용 Ray 표시
        Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
    }
    // ▲▲▲ 여기까지 수정된 부분입니다 ▲▲▲


    void HandleActions()
    {
        // (기존 코드와 동일)
        if (animator == null) return;

        if (IsTired())
        {
            animator.SetBool("IsGrooming", Input.GetKey(KeyCode.Q));
            Debug.Log("피로 상태: 액션 사용 불가!");
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
        animator.SetBool("IsJumping", !isGrounded);
        animator.SetBool("IsTired", IsTired());

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
    


    
    bool IsTired()
    {
        if (playerMental == null) return false;
        return playerMental.IsTired();
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Collision 감지: 착지");
        }
    }

    void OnDrawGizmos()
    {
        // (기존 코드와 동일)
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