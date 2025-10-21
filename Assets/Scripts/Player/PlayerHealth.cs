using UnityEngine;
using System.Collections; // 코루틴 사용을 위해 추가

public class PlayerHealth : MonoBehaviour
{
    [Header("체력 설정")]
    public float maxHealth = 100f;
    public float currentHealth;

    // --- [추가] 자동 체력 회복 설정 ---
    [Header("자동 체력 회복")]
    [Tooltip("자동 회복 기능을 사용할지 결정합니다.")]
    public bool useHealthRegen = true;

    [Tooltip("마지막으로 피해를 입은 후, 회복이 시작될 때까지의 대기 시간(초)")]
    public float regenDelay = 5f;

    [Tooltip("초당 회복되는 체력의 양")]
    public float regenRate = 2f;

    private Coroutine regenCoroutine;
    private float lastDamageTime;
    // --- [추가 끝] ---

    [Header("UI 연동")]
    public HealthBarUI healthBarUI;
    
    [Tooltip("LowHealthEffect 스크립트가 있는 오브젝트를 연결해주세요.")]
    public LowHealthEffect lowHealthEffect; 
    
    // --- [ ✨ 여기가 추가됐어용! ✨ ] ---
    [Tooltip("체력이 이 비율(%) 이하일 때 로우 헬스 이펙트가 발동됩니다. (예: 0.3 = 30%)")]
    [Range(0f, 1f)] // 0.0부터 1.0까지만 조절 가능한 슬라이더
    public float lowHealthThreshold = 0.3f; 
    // --- [ ✨ 추가 끝 ✨ ] ---

    private bool isLowHealth = false;

    // --- [ 파티클 변수 추가됨 ] ---
    [Header("VFX (이펙트)")]
    [Tooltip("자동 체력 회복 중에 재생될 파티클 이펙트")]
    public ParticleSystem healthRegenEffect;
    // --- [ 추가 끝 ] ---

    private bool isDead = false;
    private PlayerController playerController;
    private Animator animator;

    void Start()
    {
        currentHealth = maxHealth;
        playerController = GetComponent<PlayerController>();
        animator = GetComponent<Animator>();
        UpdateHealthUI();
        CheckLowHealthStatus();

        // 파티클이 켜진 상태로 시작하는 것을 방지
        if (healthRegenEffect != null)
        {
            healthRegenEffect.Stop();
        }
    }

    // --- [추가] Update 메서드 ---
    void Update()
    {
        // 자동 회복 로직 처리
        if (useHealthRegen && !isDead && currentHealth < maxHealth)
        {
            // 마지막 피격 시간으로부터 regenDelay만큼 지났고, 회복 코루틴이 실행 중이 아닐 때
            if (Time.time > lastDamageTime + regenDelay && regenCoroutine == null)
            {
                regenCoroutine = StartCoroutine(RegenerateHealth());
            }
        }
    }
    // --- [추가 끝] ---

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"데미지 받음! 현재 체력: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
        CheckLowHealthStatus();

        // --- [추가] 피격 시 자동 회복 중단 ---
        lastDamageTime = Time.time;
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;

            // --- [ 파티클 중단 추가됨 ] ---
            // 회복이 중단됐으니 파티클도 즉시 끈다!
            if (healthRegenEffect != null)
            {
                healthRegenEffect.Stop();
            }
            // --- [ 추가 끝 ] ---
        }
        // --- [추가 끝] ---

        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }

    public void Heal(float healAmount)
    {
        if (isDead) return;

        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"체력 회복! 현재 체력: {currentHealth}/{maxHealth}");
        UpdateHealthUI();
        CheckLowHealthStatus();
    }
    
    // --- [추가] 체력 회복 코루틴 ---
    private IEnumerator RegenerateHealth()
    {
        Debug.Log("체력 자동 회복을 시작합니다.");

        // --- [ 파티클 시작 추가됨 ] ---
        // 회복 시작! 파티클을 켠다 (null이 아닐 때만)
        if (healthRegenEffect != null)
        {
            healthRegenEffect.Play();
        }
        // --- [ 추가 끝 ] ---

        while(currentHealth < maxHealth)
        {
            // regenRate만큼 초당 체력을 회복
            currentHealth += regenRate * Time.deltaTime;
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // 최대 체력을 넘지 않도록
            UpdateHealthUI();
            CheckLowHealthStatus();
            yield return null; // 다음 프레임까지 대기
        }

        Debug.Log("체력이 모두 회복되었습니다.");

        // --- [ 파티클 중단 추가됨 ] ---
        // 회복 끝! 파티클을 끈다 (null이 아닐 때만)
        if (healthRegenEffect != null)
        {
            healthRegenEffect.Stop();
        }
        // --- [ 추가 끝 ] ---

        regenCoroutine = null; // 코루틴이 끝났으므로 null로 초기화
    }
    // --- [추가 끝] ---

    void Die()
    {
        isDead = true;
        Debug.Log("플레이어 사망!");
        CheckLowHealthStatus();
        
        // --- [추가] 사망 시 자동 회복 중단 ---
        if (regenCoroutine != null)
        {
            StopCoroutine(regenCoroutine);
            regenCoroutine = null;

            // --- [ 파티클 중단 추가됨 ] ---
            // 사망 시에도 파티클을 끈다!
            if (healthRegenEffect != null)
            {
                healthRegenEffect.Stop();
            }
            // --- [ 추가 끝 ] ---
        }
        // --- [추가 끝] ---

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }
        
        Debug.Log("게임 오버! 플레이어가 사망했습니다.");
    }
    
    private void CheckLowHealthStatus()
    {
        if (lowHealthEffect == null) return;

        // --- [ ✨ 여기가 0.3f 대신 변수로 수정되었어용! ✨ ] ---
        // 인스펙터에서 설정한 lowHealthThreshold 값을 사용!
        if (currentHealth <= maxHealth * lowHealthThreshold && !isDead) 
        {
        // --- [ ✨ 수정 끝 ✨ ] ---
            if (!isLowHealth)
            {
                isLowHealth = true;
                lowHealthEffect.StartEffect();
            }
        }
        else
        {
            if (isLowHealth)
            {
                isLowHealth = false;
                lowHealthEffect.StopEffect();
            }
        }
    }

    void UpdateHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealthBar(currentHealth, maxHealth);
        }
        else
        {
            Debug.Log($"체력: {currentHealth}/{maxHealth}");
        }
    }

    [ContextMenu("테스트: 10 데미지")]
    public void TestDamage() { TakeDamage(10f); }
    
    [ContextMenu("테스트: 80 데미지")]
    public void TestDamage80() { TakeDamage(80f); }
    
    [ContextMenu("테스트: 20 힐")]
    public void TestHeal() { Heal(20f); }

    [ContextMenu("테스트: 치명적 데미지")]
    public void TestLethalDamage() { TakeDamage(currentHealth); }
    
    public float GetHealthPercentage() { return currentHealth / maxHealth; }
    public bool IsAlive() { return !isDead; }
    public bool IsDead() { return isDead; }
    public bool IsFullHealth() { return currentHealth >= maxHealth; }
}