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

        // ▼▼▼ [추가된 부분] 테스트용: F키를 누르면 데미지 모션 실행! ▼▼▼
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnTakeDamage();
        }
        // ▲▲▲▲▲▲

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

        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            animator.SetBool("IsJumping", true);
        }
    }

    // ▼▼▼ [새로 추가된 함수] 외부에서 이 함수를 부르면 데미지 모션이 나옵니다! ▼▼▼
    public void OnTakeDamage()
    {
        if (animator != null)
        {
            // 애니메이터에 'TakeDamage' 라는 Trigger를 발동시킴
            animator.SetTrigger("TakeDamage");
            
            // 필요하다면 여기에 "윽!" 하는 소리를 넣거나 이펙트를 추가해도 좋아용.
            Debug.Log("으악! 데미지를 입었어용!");
        }
    }
    // ▲▲▲▲▲▲

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