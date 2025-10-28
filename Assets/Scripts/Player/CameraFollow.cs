using UnityEngine;



public class CameraFollow : MonoBehaviour

{

    public Transform target;

    public Vector3 offset = new Vector3(0f, 10f, -5f);



    [Header("Zoom Settings")]

    public float zoomSpeed = 2f;

    public float minZoom = 0.5f;     // �ּ� �� (������)

    public float maxZoom = 2f;       // �ִ� �� (�ָ�)

    public float zoomSmoothTime = 0.3f; // �� �ε巯�� (�������� ����)



    [Header("Edge Pan Settings")]

    public bool enableEdgePan = true;

    public float edgePanSpeed = 5f;      // �����ڸ� �д� �ӵ�

    public float edgeThickness = 30f;    // �����ڸ� �β� (�ȼ�)

    public float maxPanDistance = 10f;   // �ִ� �д� �Ÿ�

    public float panSmoothTime = 0.3f;   // �д� �ε巯��



    private float targetZoom = 1f;   // ��ǥ �� ��

    private float currentZoom = 1f;  // ���� �� ��

    private float zoomVelocity = 0f; // SmoothDamp�� �ӵ� ����



    private Vector3 panOffset = Vector3.zero;    // �д� ������

    private Vector3 targetPanOffset = Vector3.zero; // ��ǥ �д� ������

    private Vector3 panVelocity = Vector3.zero;  // �д� �ӵ�



    void LateUpdate()

    {

        if (target != null)

        {

            HandleZoom();

            HandleEdgePan();

            HandleCenterReset();



            // ���� ī�޶� ��ġ ���

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



        // �ε巴�� �� ����

        currentZoom = Mathf.SmoothDamp(currentZoom, targetZoom, ref zoomVelocity, zoomSmoothTime);

    }



    void HandleEdgePan()

    {

        if (!enableEdgePan) return;



        Vector3 mousePos = Input.mousePosition;

        Vector3 panDirection = Vector3.zero;



        // ȭ�� �����ڸ� ����

        if (mousePos.x <= edgeThickness)

        {

            panDirection.x = -1f; // ����

        }

        else if (mousePos.x >= Screen.width - edgeThickness)

        {

            panDirection.x = 1f; // ������

        }



        if (mousePos.y <= edgeThickness)

        {

            panDirection.z = -1f; // �Ʒ��� (3D������ Z��)

        }

        else if (mousePos.y >= Screen.height - edgeThickness)

        {

            panDirection.z = 1f; // ����

        }



        // �д� ������ ������ ����

        if (panDirection.magnitude > 0f)

        {

            targetPanOffset += panDirection * edgePanSpeed * Time.deltaTime;



            // �ִ� �д� �Ÿ� ����

            targetPanOffset = Vector3.ClampMagnitude(targetPanOffset, maxPanDistance);

        }



        // �ε巴�� �д� ����

        panOffset = Vector3.SmoothDamp(panOffset, targetPanOffset, ref panVelocity, panSmoothTime);

    }



    void HandleCenterReset()

    {

        // ���콺 �߾� �� Ŭ�� (Button 2)

        if (Input.GetMouseButtonDown(2))

        {

            // �÷��̾� �߽����� ����

            targetPanOffset = Vector3.zero;

        }

    }



    // ����׿� - Scene �信�� �д� ���� ǥ��

    void OnDrawGizmos()

    {

        if (target == null) return;



        // �÷��̾� ��ġ ǥ��

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(target.position, 0.5f);



        // ���� ī�޶� Ÿ�� ��ġ ǥ��

        Vector3 baseTarget = target.position + offset * currentZoom;

        Gizmos.color = Color.blue;

        Gizmos.DrawWireSphere(baseTarget, 0.3f);



        // �д׵� ���� ��ġ ǥ��

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(baseTarget + panOffset, 0.3f);



        // �д� ���� ǥ��

        if (panOffset.magnitude > 0.1f)

        {

            Gizmos.color = Color.yellow;

            Gizmos.DrawLine(baseTarget, baseTarget + panOffset);

        }



        // �ִ� �д� ���� ǥ��

        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(baseTarget, maxPanDistance);

    }

}

