using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0f, 10f, -5f);

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoom = 0.5f;     // 최소 줌 (가까이)
    public float maxZoom = 2f;       // 최대 줌 (멀리)
    public float zoomSmoothTime = 0.3f; // 줌 부드러움 (낮을수록 빠름)

    [Header("Edge Pan Settings")]
    public bool enableEdgePan = true;
    public float edgePanSpeed = 5f;      // 가장자리 패닝 속도
    public float edgeThickness = 30f;    // 가장자리 두께 (픽셀)
    public float maxPanDistance = 10f;   // 최대 패닝 거리
    public float panSmoothTime = 0.3f;   // 패닝 부드러움

    private float targetZoom = 1f;   // 목표 줌 값
    private float currentZoom = 1f;  // 현재 줌 값
    private float zoomVelocity = 0f; // SmoothDamp용 속도 변수

    private Vector3 panOffset = Vector3.zero;    // 패닝 오프셋
    private Vector3 targetPanOffset = Vector3.zero; // 목표 패닝 오프셋
    private Vector3 panVelocity = Vector3.zero;  // 패닝 속도

    void LateUpdate()
    {
        if (target != null)
        {
            HandleZoom();
            HandleEdgePan();
            HandleCenterReset();

            // 최종 카메라 위치 계산
            Vector3 basePosition = target.position + offset * currentZoom;
            transform.position = basePosition + panOffset;
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            targetZoom -= scroll * zoomSpeed;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        // 부드럽게 줌 적용
        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);
    }

    void HandleEdgePan()
    {
        if (!enableEdgePan) return;

        Vector3 mousePos = Input.mousePosition;
        Vector3 panDirection = Vector3.zero;

        // 화면 가장자리 감지
        if (mousePos.x <= edgeThickness)
        {
            panDirection.x = -1f; // 왼쪽
        }
        else if (mousePos.x >= Screen.width - edgeThickness)
        {
            panDirection.x = 1f; // 오른쪽
        }

        if (mousePos.y <= edgeThickness)
        {
            panDirection.z = -1f; // 아래쪽 (3D에서는 Z축)
        }
        else if (mousePos.y >= Screen.height - edgeThickness)
        {
            panDirection.z = 1f; // 위쪽
        }

        // 패닝 방향이 있으면 적용
        if (panDirection.magnitude > 0f)
        {
            targetPanOffset += panDirection * edgePanSpeed * Time.deltaTime;

            // 최대 패닝 거리 제한
            targetPanOffset = Vector3.ClampMagnitude(targetPanOffset, maxPanDistance);
        }

        // 부드럽게 패닝 적용
        panOffset = Vector3.SmoothDamp(panOffset, targetPanOffset, ref panVelocity, panSmoothTime);
    }

    void HandleCenterReset()
    {
        // 마우스 중앙 휠 클릭 (Button 2)
        if (Input.GetMouseButtonDown(2))
        {
            // 플레이어 중심으로 복귀
            targetPanOffset = Vector3.zero;
        }
    }

    // 디버그용 - Scene 뷰에서 패닝 정보 표시
    void OnDrawGizmos()
    {
        if (target == null) return;

        // 플레이어 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(target.position, 0.5f);

        // 현재 카메라 타겟 위치 표시
        Vector3 baseTarget = target.position + offset * currentZoom;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(baseTarget, 0.3f);

        // 패닝된 최종 위치 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(baseTarget + panOffset, 0.3f);

        // 패닝 방향 표시
        if (panOffset.magnitude > 0.1f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(baseTarget, baseTarget + panOffset);
        }

        // 최대 패닝 범위 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(baseTarget, maxPanDistance);
    }
}