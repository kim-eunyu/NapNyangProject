using UnityEngine;
using System.Collections;

public class PlayerMental : MonoBehaviour
{
    [Header("정신력 설정")]
    public float maxMentalHealth = 100f;
    public float currentMentalHealth;
    public float mentalDecayRate = 0.56f; // 초당 0.56씩 감소 (3분)
    public float groomingRecoveryRate = 20f; // 그루밍 시 초당 20씩 회복 (5초)

    [Header("UI 참조")]
    public MentalBarUI mentalHealthUI; // 정신력 UI

    private bool isGrooming = false;
    private bool isTired = false; // 정신력 고갈 상태
    private PlayerController playerController;
    private Animator animator;

    void Start()
    {
        // 초기 정신력 설정
        currentMentalHealth = maxMentalHealth;

        // 컴포넌트 참조
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();

        // 정신력 UI 업데이트
        UpdateMentalUI();

        // 정신력 감소 코루틴 시작
        StartCoroutine(MentalHealthDecay());
    }

    void Update()
    {
        CheckGroomingStatus();
    }

    void CheckGroomingStatus()
    {
        // 그루밍 상태 확인 (Q키)
        bool wasGrooming = isGrooming;
        isGrooming = Input.GetKey(KeyCode.Q);

        // 그루밍 시작/종료 로그
        if (isGrooming && !wasGrooming)
        {
            Debug.Log("그루밍 시작 - 정신력 회복 중!");
            if (mentalHealthUI != null)
                mentalHealthUI.ShowGroomingEffect();
        }
        else if (!isGrooming && wasGrooming)
        {
            Debug.Log("그루밍 종료");
            if (mentalHealthUI != null)
                mentalHealthUI.HideGroomingEffect();
        }
    }

    IEnumerator MentalHealthDecay()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초마다 실행

            if (isGrooming)
            {
                // 그루밍 중이면 정신력 회복
                RecoverMentalHealth(groomingRecoveryRate);
            }
            else
            {
                // 그루밍 중이 아니면 정신력 감소
                DecreaseMentalHealth(mentalDecayRate);
            }
        }
    }

    // 정신력 감소
    public void DecreaseMentalHealth(float amount)
    {
        currentMentalHealth -= amount;
        currentMentalHealth = Mathf.Clamp(currentMentalHealth, 0, maxMentalHealth);

        UpdateMentalUI();

        // 정신력 고갈 체크
        CheckTiredState();
    }

    // 정신력 회복
    public void RecoverMentalHealth(float amount)
    {
        currentMentalHealth += amount;
        currentMentalHealth = Mathf.Clamp(currentMentalHealth, 0, maxMentalHealth);

        UpdateMentalUI();

        // 정신력 회복 체크
        CheckTiredState();
    }

    // 피로 상태 확인 및 처리
    void CheckTiredState()
    {
        bool wasTired = isTired;
        isTired = currentMentalHealth <= 0;

        // 피로 상태 변화 시
        if (isTired && !wasTired)
        {
            // 피로 상태 시작
            Debug.Log("정신력 고갈! 피로 상태 시작");
            OnTiredStart();
        }
        else if (!isTired && wasTired)
        {
            // 피로 상태 종료
            Debug.Log("정신력 회복! 피로 상태 종료");
            OnTiredEnd();
        }
    }

    // 피로 상태 시작
    void OnTiredStart()
    {
        if (animator != null)
        {
            animator.SetBool("IsTired", true);
        }

        // 플레이어 능력 제한 (PlayerController에서 참조)
    }

    // 피로 상태 종료  
    void OnTiredEnd()
    {
        if (animator != null)
        {
            animator.SetBool("IsTired", false);
        }
    }

    // 정신력 UI 업데이트
    void UpdateMentalUI()
    {
        if (mentalHealthUI != null)
        {
            mentalHealthUI.UpdateMentalHealthBar(currentMentalHealth, maxMentalHealth);
        }
        else
        {
            Debug.Log($"정신력: {currentMentalHealth:F1}/{maxMentalHealth}"); // UI 없을 때 로그로 표시
        }
    }

    // 테스트용 메서드들 (Inspector에서 우클릭으로 실행 가능)
    [ContextMenu("테스트: 10 정신력 감소")]
    public void TestMentalDamage()
    {
        DecreaseMentalHealth(10f);
    }

    [ContextMenu("테스트: 20 정신력 회복")]
    public void TestMentalRecover()
    {
        RecoverMentalHealth(20f);
    }

    [ContextMenu("테스트: 정신력 모두 소모")]
    public void TestMentalEmpty()
    {
        DecreaseMentalHealth(currentMentalHealth);
    }

    // 외부에서 호출 가능한 메서드들
    public float GetMentalHealthPercentage()
    {
        return currentMentalHealth / maxMentalHealth;
    }

    public bool IsMentallyHealthy()
    {
        return currentMentalHealth > 0;
    }

    public bool IsGrooming()
    {
        return isGrooming;
    }

    public bool IsTired()
    {
        return isTired;
    }
}