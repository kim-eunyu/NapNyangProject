using UnityEngine;
using UnityEngine.UI;

public class MentalBarUI : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Image mentalBarFill; // 정신력 원형 게이지 Fill
    public Image mentalBarBackground; // 정신력 원형 배경

    [Header("시각 효과")]
    public bool enablePulseEffect = true; // 낮을 때 깜빡임 효과
    public float pulseThreshold = 0.3f; // 깜빡임 시작 임계값
    public float pulseSpeed = 2f; // 깜빡임 속도

    private float currentMental;
    private float maxMental;
    private bool isPulsing = false;
    private Color originalFillColor;

    void Start()
    {
        // 원형 게이지 설정
        if (mentalBarFill != null)
        {
            mentalBarFill.type = Image.Type.Filled;
            mentalBarFill.fillMethod = Image.FillMethod.Radial360;
            mentalBarFill.fillOrigin = (int)Image.Origin360.Top; // 12시 방향부터 시작
            mentalBarFill.fillClockwise = true; // 시계 방향

            // 원래 색상 저장
            originalFillColor = mentalBarFill.color;
        }

        Debug.Log("원형 정신력 게이지 초기화 완료!");
    }

    void Update()
    {
        // 펄스 효과 처리
        if (enablePulseEffect && isPulsing)
        {
            HandlePulseEffect();
        }
    }

    public void UpdateMentalHealthBar(float current, float max)
    {
        currentMental = current;
        maxMental = max;

        float mentalPercent = current / max;

        // Fill Amount 업데이트 (원형)
        if (mentalBarFill != null)
        {
            mentalBarFill.fillAmount = mentalPercent;
        }

        // 펄스 효과 조건 판단
        bool shouldPulse = mentalPercent <= pulseThreshold;

        // 상태 변경 시 처리
        if (isPulsing != shouldPulse)
        {
            isPulsing = shouldPulse;

            if (!isPulsing && mentalBarFill != null)
            {
                // 펄스 종료 → 원래 색과 알파 복원
                Color restoredColor = originalFillColor;
                restoredColor.a = 1f;
                mentalBarFill.color = restoredColor;
                Debug.Log("펄스 종료 → 알파 복원 완료");
            }
        }

        Debug.Log($"원형 정신력 UI 업데이트: {mentalPercent:P0}");
    }

    void HandlePulseEffect()
    {
        if (mentalBarFill == null) return;

        // 알파 값을 사인파로 변경하여 깜빡임 효과
        float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * pulseSpeed);
        Color currentColor = originalFillColor;
        currentColor.a = alpha;
        mentalBarFill.color = currentColor;
    }

    // 그루밍 중일 때 시각 효과
    public void ShowGroomingEffect()
    {
        if (mentalBarFill != null)
        {
            Color greenColor = Color.green;
            greenColor.a = mentalBarFill.color.a; // 현재 알파 유지
            mentalBarFill.color = greenColor;
        }
    }

    public void HideGroomingEffect()
    {
        if (mentalBarFill != null)
        {
            Color normalColor = originalFillColor;
            normalColor.a = mentalBarFill.color.a; // 현재 알파 유지
            mentalBarFill.color = normalColor;
        }
    }
}
