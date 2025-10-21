using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Canvas healthCanvas; // World Space Canvas
    public Image healthBarFill; // 체력바 Fill 이미지
    public Image healthBarBackground; // 체력바 배경

    [Header("색상 설정")]
    public Color fullHealthColor = Color.green;    // 체력 풀일 때 (초록색)
    public Color halfHealthColor = Color.yellow;   // 체력 절반일 때 (노란색)
    public Color lowHealthColor = Color.red;       // 체력 낮을 때 (빨간색)

    [Header("설정")]
    public Vector3 offset = new Vector3(0, 2f, 0); // 캐릭터로부터의 오프셋
    public bool hideWhenFull = true; // 체력이 풀일 때 숨기기
    public float hideDelay = 3f; // 풀 체력 후 숨기기까지의 시간

    private Transform player;
    private Camera mainCamera;
    private float lastDamageTime;
    private bool isVisible = true;

    void Start()
    {
        // 카메라 찾기
        mainCamera = Camera.main;

        // Canvas 설정
        if (healthCanvas == null)
            healthCanvas = GetComponent<Canvas>();

        if (healthCanvas != null)
        {
            healthCanvas.renderMode = RenderMode.WorldSpace;
            healthCanvas.worldCamera = mainCamera;

            // Canvas 크기를 정상 크기로 (너무 작지 않게)
            healthCanvas.transform.localScale = Vector3.one;
        }

        // 강제로 보이기 (테스트용)
        SetVisible(true);
        Debug.Log("체력바 초기화 완료!");
    }

    void Update()
    {
        // 카메라를 향해 회전 (빌보드 효과)
        if (mainCamera != null)
        {
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                           mainCamera.transform.rotation * Vector3.up);
        }

        // Player의 하위 오브젝트이므로 자동으로 따라다님

        // 자동 숨기기 처리
        if (hideWhenFull && isVisible && Time.time - lastDamageTime > hideDelay)
        {
            if (healthBarFill != null && healthBarFill.fillAmount >= 1f)
            {
                SetVisible(false);
            }
        }
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        if (healthBarFill == null) return;

        float healthPercent = currentHealth / maxHealth;
        healthBarFill.fillAmount = healthPercent;

        // 체력 퍼센트에 따른 색상 변경
        Color targetColor = GetHealthColor(healthPercent);
        healthBarFill.color = targetColor;

        // 항상 보이게 (테스트용)
        SetVisible(true);
        lastDamageTime = Time.time;

        Debug.Log($"체력바 업데이트: {healthPercent:P0}, 색상: {targetColor}");
    }

    Color GetHealthColor(float healthPercent)
    {
        if (healthPercent > 0.6f)
        {
            // 100% ~ 60%: 초록색 → 노란색으로 변화
            float t = (1f - healthPercent) / 0.4f; // 0~1로 정규화
            return Color.Lerp(fullHealthColor, halfHealthColor, t);
        }
        else if (healthPercent > 0.3f)
        {
            // 60% ~ 30%: 노란색 → 빨간색으로 변화
            float t = (0.6f - healthPercent) / 0.3f; // 0~1로 정규화
            return Color.Lerp(halfHealthColor, lowHealthColor, t);
        }
        else
        {
            // 30% 이하: 빨간색
            return lowHealthColor;
        }
    }

    void SetVisible(bool visible)
    {
        if (healthCanvas != null)
            healthCanvas.gameObject.SetActive(visible);
        isVisible = visible;
    }

    // 강제로 보이기/숨기기
    public void Show()
    {
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }
}