using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("타겟")]
    public Transform target; // 플레이어 Transform

    [Header("속도 및 감도")]
    public float rotationSpeed = 150.0f; // 마우스 회전 속도
    public float zoomSpeed = 5.0f;      // 줌 속도
    public float rotationSmoothTime = 0.12f; // 회전 부드러움

    [Header("거리 및 각도 제한")]
    public float distance = 5.0f;     // 카메라와 타겟의 거리
    public float minDistance = 1.5f;    // 최소 줌 거리
    public float maxDistance = 10.0f;   // 최대 줌 거리
    public float minYAngle = -20.0f;  // 최소 Y각도 (아래)
    public float maxYAngle = 80.0f;   // 최대 Y각도 (위)

    [Header("충돌 처리")]
    public LayerMask collisionLayers; // 카메라가 충돌할 레이어 (예: "Wall", "Ground")
    public float collisionPadding = 0.2f; // 충돌 시 벽에서 살짝 띄울 거리

    private float currentX = 0.0f; 
    private float currentY = 0.0f; 
    private float targetX = 0.0f;
    private float targetY = 0.0f;
    private float targetDistance = 0.0f;

    private float xVelocity = 0.0f;
    private float yVelocity = 0.0f;
    private float zoomVelocity = 0.0f;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("카메라 타겟이 설정되지 않았습니다!");
            return;
        }

        Vector3 angles = transform.eulerAngles;
        targetX = currentX = angles.y;
        targetY = currentY = angles.x;
        targetDistance = distance;

        // <--- 수정!
        // 시작할 때부터 항상 커서를 보이게 합니다.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void LateUpdate()
    {
        // <--- 수정!
        // Tab 키 토글 로직, 커서 잠금 확인 로직 '전부 삭제'

        if (target == null) return;

        // 1. 마우스 입력 받기
        // (경고: 커서가 보여도 마우스 입력은 계속 받습니다!)
        targetX += Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        targetY -= Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // 2. Y각도 제한
        targetY = Mathf.Clamp(targetY, minYAngle, maxYAngle);

        // 3. 줌(스크롤) 입력 받기
        targetDistance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // 4. 회전값 부드럽게 적용
        currentX = Mathf.SmoothDamp(currentX, targetX, ref xVelocity, rotationSmoothTime);
        currentY = Mathf.SmoothDamp(currentY, targetY, ref yVelocity, rotationSmoothTime);
        distance = Mathf.SmoothDamp(distance, targetDistance, ref zoomVelocity, 0.1f);

        // 5. 최종 회전값 계산
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);

        // 6. 카메라 위치 계산
        Vector3 desiredPosition = target.position - (rotation * new Vector3(0, 0, distance));

        // 7. 충돌 처리
        RaycastHit hit;
        Vector3 direction = desiredPosition - target.position;
        float rayDistance = Vector3.Distance(target.position, desiredPosition);

        if (Physics.Raycast(target.position, direction.normalized, out hit, rayDistance, collisionLayers))
        {
            transform.position = hit.point + hit.normal * collisionPadding;
        }
        else
        {
            transform.position = desiredPosition;
        }

        // 8. 카메라가 타겟을 바라보게 설정
        transform.LookAt(target.position);
    }
}