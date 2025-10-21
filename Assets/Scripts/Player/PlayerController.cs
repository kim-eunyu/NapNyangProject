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
    private bool isRunning;
    private bool isGrounded = true;
    private PlayerMental playerMental; // 정신력 컴포넌트 참조

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMental = GetComponent<PlayerMental>(); // 정신력 컴포넌트 가져오기

        // Rigidbody 설정 최적화
        rb.freezeRotation = true;
        rb.linearDamping = 5f; // 공기 저항 추가 (미끄러짐 방지)
    }

    void Update()
    {
        HandleMovementInput();
        HandleActions();
        HandleAnimations();
        CheckGroundWithRaycast(); // 더 정확한 지면 체크
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
        moveInput = new Vector3(h, 0, v).normalized;

        // 피로 상태일 때는 달리기 불가능
        if (IsTired())
        {
            isRunning = false;
        }
        else
        {
            isRunning = Input.GetKey(KeyCode.LeftShift);
        }
    }

    void HandleMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            float moveSpeed = isRunning ? runSpeed : walkSpeed;

            // 피로 상태일 때는 이동속도 반으로 감소
            if (IsTired())
            {
                moveSpeed = walkSpeed * 0.5f; // 반속도로 천천히 이동
            }

            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;

            // 속도 기반 이동으로 변경 (MovePosition 대신)
            Vector3 targetVelocity = moveDirection * moveSpeed;
            targetVelocity.y = rb.linearVelocity.y; // Y축 속도 유지 (중력/점프)

            rb.linearVelocity = targetVelocity;
        }
        else
        {
            // 정지 시 수평 속도만 0으로
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    void HandleRotation()
    {
        if (moveInput.magnitude > 0.1f)
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);

            // 피로 상태일 때는 회전도 약간 느리게
            float currentRotationSpeed = IsTired() ? rotationSpeed * 0.7f : rotationSpeed;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                                                currentRotationSpeed * Time.fixedDeltaTime);
        }
    }

    void CheckGroundWithRaycast()
    {
        // Raycast로 더 정확한 지면 감지
        float rayDistance = 0.3f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;

        RaycastHit hit;
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance);

        // 지면 상태 변화 로그
        if (wasGrounded != isGrounded)
        {
            Debug.Log(isGrounded ? "지면 감지!" : "공중으로!");
        }

        // 디버그용 Ray 표시
        Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, isGrounded ? Color.green : Color.red);
    }

    void HandleActions()
    {
        if (animator == null) return;

        // 피로 상태일 때는 액션 스킬 사용 불가
        if (IsTired())
        {
            // 피로 상태에서는 그루밍만 가능
            animator.SetBool("IsGrooming", Input.GetKey(KeyCode.Q));

            // 다른 액션들은 모두 차단
            Debug.Log("피로 상태: 액션 사용 불가! 그루밍으로 정신력을 회복하세요.");
            return;
        }

        // 정상 상태일 때만 모든 액션 가능
        if (Input.GetMouseButtonDown(0))
            animator.SetTrigger("Attack");
        if (Input.GetMouseButtonDown(1))
            animator.SetTrigger("Skill");
        if (Input.GetKeyDown(KeyCode.R))
            animator.SetTrigger("Ultimate");

        animator.SetBool("IsHiding", Input.GetKey(KeyCode.LeftControl));
        animator.SetBool("IsGrooming", Input.GetKey(KeyCode.Q));

        // 피로 상태가 아닐 때만 점프 가능
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            Debug.Log($"점프! 현재 속도: {rb.linearVelocity}");
        }
    }

    void HandleAnimations()
    {
        if (animator == null) return;

        float speedVal = 0f;
        if (moveInput.magnitude > 0.1f)
        {
            if (IsTired())
            {
                // 피로 상태일 때는 느린 걷기만 (속도 0.5)
                speedVal = 0.5f;
            }
            else
            {
                // 정상 상태일 때
                speedVal = isRunning ? 2f : 1f;
            }
        }

        animator.SetFloat("Speed", speedVal);
        animator.SetBool("IsJumping", !isGrounded);

        // 피로 상태 애니메이션 파라미터
        animator.SetBool("IsTired", IsTired());
    }

    // 피로 상태 확인
    bool IsTired()
    {
        if (playerMental == null) return false;
        return playerMental.IsTired();
    }

    // Collision 이벤트는 백업용으로 유지
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Collision 감지: 착지");
        }
    }

    void OnDrawGizmos()
    {
        if (moveInput.magnitude > 0.1f)
        {
            Vector3 moveDirection = new Vector3(moveInput.x, 0, moveInput.z).normalized;

            // 피로 상태일 때는 빨간색으로 표시
            Gizmos.color = IsTired() ? Color.red : Color.green;
            Gizmos.DrawRay(transform.position, moveDirection * 2f);
        }
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * 1.5f);

        // 지면 체크 Ray 표시
        Gizmos.color = isGrounded ? Color.blue : Color.yellow;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.1f;
        Gizmos.DrawRay(rayOrigin, Vector3.down * 0.3f);
    }
}