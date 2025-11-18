using UnityEngine;
using UnityEngine.UI;

public class MentalBarUI : MonoBehaviour
{
    [Header("UI ������Ʈ")]
    public Image mentalBarFill; // ���ŷ� ���� ������ Fill
    public Image mentalBarBackground; // ���ŷ� ���� ���

    [Header("�ð� ȿ��")]
    public bool enablePulseEffect = true; // ���� �� ������ ȿ��
    public float pulseThreshold = 0.3f; // ������ ���� �Ӱ谪
    public float pulseSpeed = 2f; // ������ �ӵ�

    private float currentMental;
    private float maxMental;
    private bool isPulsing = false;
    private Color originalFillColor;

    void Start()
    {
        // ���� ������ ����
        if (mentalBarFill != null)
        {
            mentalBarFill.type = Image.Type.Filled;
            mentalBarFill.fillMethod = Image.FillMethod.Radial360;
            mentalBarFill.fillOrigin = (int)Image.Origin360.Top; // 12�� ������� ����
            mentalBarFill.fillClockwise = true; // �ð� ����

            // ���� ���� ����
            originalFillColor = mentalBarFill.color;
        }

        Debug.Log("���� ���ŷ� ������ �ʱ�ȭ �Ϸ�!");
    }

    void Update()
    {
        // �޽� ȿ�� ó��
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

        // Fill Amount ������Ʈ (����)
        if (mentalBarFill != null)
        {
            mentalBarFill.fillAmount = mentalPercent;
        }

        // �޽� ȿ�� ���� �Ǵ�
        bool shouldPulse = mentalPercent <= pulseThreshold;

        // ���� ���� �� ó��
        if (isPulsing != shouldPulse)
        {
            isPulsing = shouldPulse;

            if (!isPulsing && mentalBarFill != null)
            {
                // �޽� ���� �� ���� ���� ���� ����
                Color restoredColor = originalFillColor;
                restoredColor.a = 1f;
                mentalBarFill.color = restoredColor;
                Debug.Log("�޽� ���� �� ���� ���� �Ϸ�");
            }
        }

    }

    void HandlePulseEffect()
    {
        if (mentalBarFill == null) return;

        // ���� ���� �����ķ� �����Ͽ� ������ ȿ��
        float alpha = 0.7f + 0.3f * Mathf.Sin(Time.time * pulseSpeed);
        Color currentColor = originalFillColor;
        currentColor.a = alpha;
        mentalBarFill.color = currentColor;
    }

    // �׷�� ���� �� �ð� ȿ��
    public void ShowGroomingEffect()
    {
        if (mentalBarFill != null)
        {
            Color greenColor = Color.green;
            greenColor.a = mentalBarFill.color.a; // ���� ���� ����
            mentalBarFill.color = greenColor;
        }
    }

    public void HideGroomingEffect()
    {
        if (mentalBarFill != null)
        {
            Color normalColor = originalFillColor;
            normalColor.a = mentalBarFill.color.a; // ���� ���� ����
            mentalBarFill.color = normalColor;
        }
    }
}
