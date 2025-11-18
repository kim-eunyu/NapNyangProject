using UnityEngine;
using System.Collections;

public class PlayerMental : MonoBehaviour
{
    [Header("정신력 설정")]
    public float maxMentalHealth = 100f;
    public float currentMentalHealth;
    public float mentalDecayRate = 0.56f; // 초당 0.56씩 감소 (3분)
    public float groomingRecoveryRate = 20f; // 그루밍 시 초당 20씩 회복 (5초)

    [Header("UI 연동")]
    public MentalBarUI mentalHealthUI; // 정신력 UI

    [Header("효과 연동")]
    [Tooltip("LowMentalEffect 스크립트가 있는 EffectController를 연결해주세요.")]
    public LowMentalEffect lowMentalEffect;

    private bool isGrooming = false;
    private bool isTired = false; // 정신력 0 상태
    private PlayerController playerController;
    private Animator animator;

    void Start()
    {
        currentMentalHealth = maxMentalHealth;
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        UpdateMentalUI();

        // 시작할 때 비네팅 상태 0으로 초기화
        UpdateVignetteEffect(); 

        // 정신력 감소 코루틴 시작
        StartCoroutine(MentalHealthDecay());
    }

    void Update()
    {
        CheckGroomingStatus();
    }

    void CheckGroomingStatus()
    {
        bool wasGrooming = isGrooming;
        isGrooming = Input.GetKey(KeyCode.Q);

        if (isGrooming && !wasGrooming)
        {
            Debug.Log("그루밍 시작 - 정신력 회복 중!");
            if (mentalHealthUI != null)
                mentalHealthUI.ShowGroomingEffect();
        }
        else if (!isGrooming && wasGrooming)
        {
            Debug.Log("그루밍 중지");
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
                RecoverMentalHealth(groomingRecoveryRate);
            }
            else
            {
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
        CheckTiredState();

        // 정신력 감소 시 비네팅 업데이트
        UpdateVignetteEffect();
    }

    // 정신력 회복
    public void RecoverMentalHealth(float amount)
    {
        currentMentalHealth += amount;
        currentMentalHealth = Mathf.Clamp(currentMentalHealth, 0, maxMentalHealth);

        UpdateMentalUI();
        CheckTiredState();

        // 정신력 회복 시 비네팅 업데이트
        UpdateVignetteEffect();
    }

    // 피로 상태 확인 및 처리
    void CheckTiredState()
    {
        bool wasTired = isTired;
        isTired = currentMentalHealth <= 0;

        if (isTired && !wasTired)
        {
            Debug.Log("정신력 0! 피로 상태 시작");
            OnTiredStart();
        }
        else if (!isTired && wasTired)
        {
            Debug.Log("정신력 회복! 피로 상태 해제");
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
        // (PlayerController에서 속도 저하 등 처리)
    }

    // 피로 상태 해제  
    void OnTiredEnd()
    {
        if (animator != null)
        {
            animator.SetBool("IsTired", false);
        }
    }

    // --- [추가] --- 비네팅 효과를 서서히 조절하는 새 함수
    void UpdateVignetteEffect()
    {
        if (lowMentalEffect == null) return;

        // 정신력이 20% 이하일 때만 계산 시작
        float effectThreshold = maxMentalHealth * 0.2f;
        float effectPercentage = 0f; // 기본값 (효과 없음)

        if (currentMentalHealth <= effectThreshold)
        {
            // 현재 정신력이 20%일 때 0.0이 되고, 0%일 때 1.0이 되는 값을 계산해요.
            effectPercentage = Mathf.InverseLerp(effectThreshold, 0f, currentMentalHealth);
        }

        // LowMentalEffect 스크립트로 0.0 ~ 1.0 사이의 값을 전달
        lowMentalEffect.UpdateEffect(effectPercentage);
    }
    // --- [추가 끝] ---

    // 정신력 UI 업데이트
    void UpdateMentalUI()
    {
        if (mentalHealthUI != null)
        {
            mentalHealthUI.UpdateMentalHealthBar(currentMentalHealth, maxMentalHealth);
        }
       
    }

    // 테스트용 함수들
    [ContextMenu("테스트: 10 정신력 감소")]
    public void TestMentalDamage() { DecreaseMentalHealth(10f); }
    [ContextMenu("테스트: 20 정신력 회복")]
    public void TestMentalRecover() { RecoverMentalHealth(20f); }
    [ContextMenu("테스트: 정신력 모두 소모")]
    public void TestMentalEmpty() { DecreaseMentalHealth(currentMentalHealth); }

    // 외부 호출 가능 함수들
    public float GetMentalHealthPercentage() { return currentMentalHealth / maxMentalHealth; }
    public bool IsMentallyHealthy() { return currentMentalHealth > 0; }
    public bool IsGrooming() { return isGrooming; }
    public bool IsTired() { return isTired; }
}