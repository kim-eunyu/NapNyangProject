using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BossAI : MonoBehaviour
{
    [Header("Boss Settings")]
    public float detectionRange = 10f;
    public float maxHealth = 100f;
    public float lowHealthThreshold = 0.3f; // 30%

    [Header("Detection Settings")]
    [Tooltip("플레이어 감지 범위")]
    public float playerDetectionRadius = 10f;
    [Tooltip("감지 범위를 시각적으로 표시할지 여부")]
    public bool showDetectionRange = true;

    [Header("Attack Settings")]
    public float basicAttackInterval = 2f;
    public float areaAttackInterval = 8f;
    public float lowHealthAreaAttackInterval = 4f;
    public float basicAttackRange = 2f;
    public float basicAttackDamage = 20f;
    public float areaAttackDamage = 30f;

    [Header("Area Attack Settings")]
    [Tooltip("광역 스킬의 최소 범위")]
    public float areaAttackMinRange = 3f;
    [Tooltip("광역 스킬의 최대 범위")]
    public float areaAttackMaxRange = 8f;
    [Tooltip("광역 스킬 범위를 시각적으로 표시할지 여부")]
    public bool showAreaAttackRange = true;
    [Tooltip("광역 스킬이 실행될 위치 (0=보스 중심, 1=플레이어 위치, 0.5=중간지점)")]
    [Range(0f, 1f)]
    public float areaAttackTargetBlend = 0.5f;

    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float returnSpeed = 2f;

    [Header("Territory Settings")]
    [Tooltip("보스의 초기 위치 (비워두면 시작 위치 사용)")]
    public Transform initialPositionTransform;
    [Tooltip("초기 위치를 중심으로 한 활동 영역 반경")]
    public float territoryRadius = 15f;
    [Tooltip("영역을 시각적으로 표시할지 여부")]
    public bool showTerritoryRange = true;

    [Header("Particle Effects")]
    [Tooltip("광역 공격 경고 파티클 프리팹")]
    public GameObject areaAttackWarningParticle;
    [Tooltip("광역 공격 폭발 파티클 프리팹")]
    public GameObject areaAttackExplosionParticle;

    // Components
    private Animator animator;
    private NavMeshAgent navAgent;
    private Transform player;
    private Vector3 initialPosition;

    // State Management
    public enum BossState
    {
        Idle,
        FirstEncounter,
        Combat,
        Returning,
        Dead
    }

    private BossState currentState = BossState.Idle;
    public float currentHealth; // public으로 변경
    private bool hasEncounteredPlayer = false;
    private bool isLowHealth = false;

    // Timers
    private float basicAttackTimer;
    private float areaAttackTimer;
    private bool isAttacking = false;
    private bool isDamaged = false;

    // Area Attack Variables
    private float currentAreaAttackRange;
    private Vector3 currentAreaAttackCenter;

    // Animation State Hashes (성능 최적화)
    private readonly int idleHash = Animator.StringToHash("Idle");
    private readonly int angryHash = Animator.StringToHash("Angry");
    private readonly int walkHash = Animator.StringToHash("Walk");
    private readonly int attackHash = Animator.StringToHash("Attack");
    private readonly int areaAttackHash = Animator.StringToHash("AreaAttack");
    private readonly int areaAttackReadyHash = Animator.StringToHash("AreaAttackReady");
    private readonly int damageHash = Animator.StringToHash("Damage");
    private readonly int dieHash = Animator.StringToHash("Die");

    void Start()
    {
        // 컴포넌트 초기화
        animator = GetComponent<Animator>();
        navAgent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Rigidbody 설정 (있는 경우)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // NavMeshAgent와 충돌 방지
            rb.useGravity = false;
        }

        // NavMeshAgent 설정 (중요!)
        if (navAgent != null)
        {
            navAgent.updateRotation = false; // 수동 회전 제어
            navAgent.updatePosition = true;
            navAgent.speed = moveSpeed;
            navAgent.angularSpeed = 0f; // 자동 회전 비활성화
            navAgent.acceleration = 8f;
            navAgent.stoppingDistance = basicAttackRange * 0.5f; // 공격 범위의 절반에서 정지
            navAgent.autoBraking = true;
            navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        }

        // 초기 설정
        if (initialPositionTransform != null)
        {
            initialPosition = initialPositionTransform.position;
        }
        else
        {
            initialPosition = transform.position;
        }

        currentHealth = maxHealth;

        // 감지 범위 동기화 (하위 호환성)
        detectionRange = playerDetectionRadius;

        // 초기 상태 설정
        SetState(BossState.Idle);

        if (player == null)
        {
            Debug.LogWarning("Player not found! Make sure Player has 'Player' tag.");
        }
    }

    void Update()
    {
        if (currentState == BossState.Dead || player == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float distanceFromInitialPosition = Vector3.Distance(player.position, initialPosition); // 초기위치 기준!

        UpdateState(distanceToPlayer, distanceFromInitialPosition);
        UpdateTimers();
        CheckLowHealth();
    }

    void UpdateState(float distanceToPlayer, float distanceFromInitialPosition)
    {
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState(distanceFromInitialPosition);
                break;

            case BossState.FirstEncounter:
                HandleFirstEncounterState();
                break;

            case BossState.Combat:
                HandleCombatState(distanceToPlayer, distanceFromInitialPosition);
                break;

            case BossState.Returning:
                HandleReturningState(distanceFromInitialPosition);
                break;
        }
    }

    void HandleIdleState(float distanceFromInitialPosition)
    {
        if (distanceFromInitialPosition <= territoryRadius)
        {
            SetState(BossState.FirstEncounter);
        }
    }

    void HandleFirstEncounterState()
    {
        // 플레이어를 바라보기
        LookAtPlayer();

        // 도발 애니메이션이 끝났는지 확인
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Angry"))
        {
            SetState(BossState.Combat);
            hasEncounteredPlayer = true;
        }
    }

    void HandleCombatState(float distanceToPlayer, float distanceFromInitialPosition)
    {
        // 플레이어가 영역을 벗어났는지 확인!
        if (distanceFromInitialPosition > territoryRadius)
        {
            SetState(BossState.Returning);
            return;
        }

        if (!isAttacking && !isDamaged)
        {
            // 플레이어 추적
            if (navAgent.enabled)
            {
                navAgent.SetDestination(player.position);
                navAgent.isStopped = false;
            }

            // 자연스러운 회전 (이동 중에도 플레이어를 바라봄)
            LookAtPlayer();

            // 이동 애니메이션
            if (navAgent.velocity.magnitude > 0.1f)
            {
                PlayAnimation(walkHash);
            }
            else
            {
                PlayAnimation(idleHash);
            }

            // 공격 실행
            if (distanceToPlayer <= basicAttackRange && basicAttackTimer >= basicAttackInterval)
            {
                StartCoroutine(PerformBasicAttack());
            }
            else
            {
                float areaInterval = isLowHealth ? lowHealthAreaAttackInterval : areaAttackInterval;
                if (areaAttackTimer >= areaInterval)
                {
                    StartCoroutine(PerformAreaAttack());
                }
            }
        }
    }

    void HandleReturningState(float distanceFromInitialPosition)
    {
        // 플레이어가 다시 영역 안으로 들어왔는지 확인
        if (distanceFromInitialPosition <= territoryRadius)
        {
            SetState(BossState.FirstEncounter);
            return;
        }

        // 보스 자신의 초기 위치까지의 거리 계산
        float bossDistanceToInitial = Vector3.Distance(transform.position, initialPosition);

        // 초기 위치에 도달했는지 확인
        if (bossDistanceToInitial <= 1.5f)
        {
            // 초기 위치 도달 - Idle 상태로 전환
            navAgent.ResetPath();
            navAgent.velocity = Vector3.zero;
            navAgent.isStopped = true;

            // 정확한 위치와 회전으로 보정
            transform.position = initialPosition;
            transform.rotation = Quaternion.identity;

            SetState(BossState.Idle);
            hasEncounteredPlayer = false;

            Debug.Log("Boss returned to initial position and entered Idle state");
        }
        else
        {
            // 아직 돌아가는 중 - 계속 초기 위치로 이동
            navAgent.speed = returnSpeed;
            navAgent.isStopped = false;

            // 목적지를 지속적으로 설정 (NavMesh 문제 방지)
            if (!navAgent.pathPending && navAgent.remainingDistance < 0.1f)
            {
                navAgent.SetDestination(initialPosition);
            }

            // Walk 애니메이션
            PlayAnimation(walkHash);

            Debug.Log($"Boss returning to initial position. Distance: {bossDistanceToInitial:F1}");
        }
    }

    void SetState(BossState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case BossState.Idle:
                navAgent.speed = moveSpeed;
                navAgent.isStopped = true; // Idle 상태에서는 움직임 정지
                PlayAnimation(idleHash);
                ResetTimers();
                Debug.Log("Boss entered Idle state");
                break;

            case BossState.FirstEncounter:
                navAgent.ResetPath();
                PlayAnimation(angryHash);
                break;

            case BossState.Combat:
                navAgent.speed = moveSpeed;
                ResetTimers();
                break;

            case BossState.Returning:
                navAgent.speed = returnSpeed;
                navAgent.isStopped = false;
                // 초기 위치로 이동 설정
                if (navAgent.destination != initialPosition)
                {
                    navAgent.SetDestination(initialPosition);
                }
                Debug.Log("Boss is returning to initial position");
                break;

            case BossState.Dead:
                navAgent.enabled = false;
                PlayAnimation(dieHash);
                break;
        }
    }

    IEnumerator PerformBasicAttack()
    {
        isAttacking = true;
        navAgent.ResetPath();

        LookAtPlayer();
        PlayAnimation(attackHash);

        // 공격 애니메이션 대기
        yield return new WaitForSeconds(0.5f);

        // 현재 거리 재확인 (보스가 움직였을 수 있음)
        float currentDistance = Vector3.Distance(transform.position, player.position);
        Debug.Log($"Basic Attack - Distance: {currentDistance:F1}, Range: {basicAttackRange}");

        // 데미지 적용 (플레이어가 범위 내에 있는 경우)
        if (currentDistance <= basicAttackRange)
        {
            // PlayerHealth가 있으면 사용, 없으면 디버그 로그만
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(basicAttackDamage);
                Debug.Log($"Basic Attack HIT! Damage: {basicAttackDamage}");
            }
            else
            {
                Debug.Log($"Player hit by basic attack! Damage: {basicAttackDamage}");
            }
        }
        else
        {
            Debug.Log($"Basic Attack MISSED! Distance: {currentDistance:F1} > Range: {basicAttackRange}");
        }

        // 공격 후 대기
        yield return new WaitForSeconds(0.5f);

        basicAttackTimer = 0f;
        isAttacking = false;
    }

    IEnumerator PerformAreaAttack()
    {
        isAttacking = true;
        navAgent.ResetPath();

        // 랜덤 범위와 위치 설정
        SetRandomAreaAttackParameters();

        // 파티클 이펙트만 생성 (빨간 원 제거)
        if (areaAttackWarningParticle != null)
        {
            Vector3 particlePos = currentAreaAttackCenter + Vector3.up * 0.1f;
            GameObject warningFX = Instantiate(areaAttackWarningParticle, particlePos, Quaternion.identity);

            // 파티클 크기를 광역 공격 범위에 맞게 조정
            warningFX.transform.localScale = Vector3.one * (currentAreaAttackRange / 3f);

            Destroy(warningFX, 1.5f); // 1.5초 후 자동 삭제
        }

        LookAtPlayer();

        // 준비 애니메이션
        PlayAnimation(areaAttackReadyHash);
        yield return new WaitForSeconds(1f);

        // 광역 공격 애니메이션
        PlayAnimation(areaAttackHash);
        yield return new WaitForSeconds(0.5f);

        // 설정된 범위 내 데미지 적용
        ApplyAreaDamage();

        // 폭발 파티클 이펙트 생성 (크기 조정)
        if (areaAttackExplosionParticle != null)
        {
            Vector3 particlePos = currentAreaAttackCenter + Vector3.up * 0.1f;
            GameObject explosionFX = Instantiate(areaAttackExplosionParticle, particlePos, Quaternion.identity);

            // 파티클 크기를 광역 공격 범위에 맞게 조정
            explosionFX.transform.localScale = Vector3.one * (currentAreaAttackRange / 3f);

            Destroy(explosionFX, 3f); // 3초 후 자동 삭제
        }

        // 공격 후 대기
        yield return new WaitForSeconds(1f);

        areaAttackTimer = 0f;
        isAttacking = false;
    }

    void SetRandomAreaAttackParameters()
    {
        // 랜덤 범위 설정
        currentAreaAttackRange = Random.Range(areaAttackMinRange, areaAttackMaxRange);

        // 공격 중심점 설정 (보스와 플레이어 사이의 블렌드 포인트)
        Vector3 bossPosition = transform.position;
        Vector3 playerPosition = player.position;
        currentAreaAttackCenter = Vector3.Lerp(bossPosition, playerPosition, areaAttackTargetBlend);

        // 약간의 랜덤성 추가 (옵션)
        Vector3 randomOffset = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        );
        currentAreaAttackCenter += randomOffset;

        Debug.Log($"Area Attack - Range: {currentAreaAttackRange:F1}, Center: {currentAreaAttackCenter}");
    }

    void ApplyAreaDamage()
    {
        // 플레이어가 광역 공격 범위 내에 있는지 확인
        float distanceToAttackCenter = Vector3.Distance(player.position, currentAreaAttackCenter);

        if (distanceToAttackCenter <= currentAreaAttackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(areaAttackDamage);
                Debug.Log($"Player hit by area attack! Distance: {distanceToAttackCenter:F1}");
            }
            else
            {
                Debug.Log($"Player hit by area attack! Damage: {areaAttackDamage}, Distance: {distanceToAttackCenter:F1}");
            }
        }
        else
        {
            Debug.Log($"Player avoided area attack. Distance: {distanceToAttackCenter:F1}");
        }
    }

    public void TakeDamage(float damage)
    {
        if (currentState == BossState.Dead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        Debug.Log($"Boss took {damage} damage! Current health: {currentHealth}/{maxHealth}");

        // 피격 애니메이션
        if (!isAttacking)
        {
            StartCoroutine(PlayDamageAnimation());
        }

        // 체력이 0 이하가 되면 사망
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            SetState(BossState.Dead);
        }
    }

    IEnumerator PlayDamageAnimation()
    {
        isDamaged = true;
        navAgent.ResetPath();

        PlayAnimation(damageHash);
        yield return new WaitForSeconds(0.5f);

        isDamaged = false;
    }

    void UpdateTimers()
    {
        if (currentState == BossState.Combat && !isAttacking)
        {
            basicAttackTimer += Time.deltaTime;
            areaAttackTimer += Time.deltaTime;
        }
    }

    void CheckLowHealth()
    {
        isLowHealth = (currentHealth / maxHealth) <= lowHealthThreshold;
    }

    void ResetTimers()
    {
        basicAttackTimer = 0f;
        areaAttackTimer = 0f;
    }

    void LookAtPlayer()
    {
        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0; // Y축 회전 방지

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }
        }
    }

    void PlayAnimation(int animationHash)
    {
        if (animator != null)
        {
            animator.Play(animationHash);
        }
    }

    // 외부에서 현재 상태 확인용
    public BossState GetCurrentState()
    {
        return currentState;
    }

    // 디버그용 기즈모
    void OnDrawGizmosSelected()
    {
        Vector3 centerPos = Application.isPlaying ? initialPosition :
            (initialPositionTransform != null ? initialPositionTransform.position : transform.position);

        // 플레이어 감지 범위 (보스 중심)
        if (showDetectionRange)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, playerDetectionRadius);
        }

        // 영역 범위 (초기 위치 중심)
        if (showTerritoryRange)
        {
            Gizmos.color = new Color(1f, 0f, 1f, 0.3f); // 마젠타 (반투명)
            Gizmos.DrawWireSphere(centerPos, territoryRadius);

            // 영역 중심점 표시
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(centerPos, Vector3.one * 1.5f);
        }

        // 기본 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, basicAttackRange);

        // 광역 공격 범위 미리보기 (최소/최대)
        if (showAreaAttackRange)
        {
            Gizmos.color = new Color(0f, 0f, 1f, 0.3f); // 반투명 파랑
            Gizmos.DrawWireSphere(transform.position, areaAttackMinRange);

            Gizmos.color = new Color(0f, 0f, 1f, 0.1f); // 더 투명한 파랑
            Gizmos.DrawWireSphere(transform.position, areaAttackMaxRange);
        }

        // 현재 광역 공격 범위 (게임 실행 중일 때)
        if (Application.isPlaying && currentAreaAttackRange > 0)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentAreaAttackCenter, currentAreaAttackRange);

            // 공격 중심점 표시
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(currentAreaAttackCenter, Vector3.one * 0.5f);
        }

        // 복귀할 위치 표시
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(centerPos, Vector3.one);

        // 초기 위치까지의 연결선 (실행 중일 때)
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, centerPos);
        }
    }

    // 런타임에서 범위 확인용 (옵션)
    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // 현재 상태 표시
        switch (currentState)
        {
            case BossState.Idle:
                Gizmos.color = Color.white;
                break;
            case BossState.FirstEncounter:
                Gizmos.color = new Color(1f, 0.5f, 0f); // 오렌지색
                break;
            case BossState.Combat:
                Gizmos.color = Color.red;
                break;
            case BossState.Returning:
                Gizmos.color = Color.blue;
                break;
            case BossState.Dead:
                Gizmos.color = Color.black;
                break;
        }

        // 보스 머리 위에 상태 표시
        Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.5f);
    }
}