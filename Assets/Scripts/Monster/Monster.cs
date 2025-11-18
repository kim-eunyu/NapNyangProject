using UnityEngine;

public class Monster : MonoBehaviour
{
    // [!!! 아주 중요 !!!]
    // 1. 인스펙터에서 몬스터 종류 설정
    public MonsterType monsterType;

    [Header("Attack Settings")]
    // 2. [!!! 커서 매니저가 이 값을 읽어갈 거예용 !!!]
    public float attackDistance = 3.0f; // 몬스터를 때릴 수 있는 '사거리'

    // 3. 플레이어의 Transform (클릭 시 거리 계산용)
    private Transform playerTransform;

    // Start 함수 (플레이어 찾기)
    private void Start()
    {
        // 4. 'OnMouseDown'에서 쓸 플레이어를 찾아놔용.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError($"Monster ({name}): 'Player' 태그를 가진 플레이어를 찾을 수 없습니다!");
        }
    }

    // (Die 함수는 기존과 100% 동일해용)
    public void Die()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ReportMonsterKill(monsterType);
        }
        Debug.Log($"{monsterType} 처치!");
        Destroy(gameObject); 
    }

    
    // (OnMouseDown '클릭' 기능은 그대로 살아있어용!)
    private void OnMouseDown()
    {
        if (playerTransform == null) return; 

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        // '클릭'했을 때 사거리 체크
        if (distance <= attackDistance)
        {
            Debug.Log($"{monsterType} 클릭됨! (사거리 내) 테스트용 죽음 처리...");
            Die();
        }
        else
        {
            Debug.Log($"플레이어가 {monsterType}로부터 너무 멉니다! (거리: {distance})");
        }
    }
    
    // --- [!!! 삭제됨 !!!] ---
    // isMouseOver, OnMouseEnter, OnMouseExit, Update 함수가
    // 싹! 사라졌어용!
    // (이제 이 '실시간 커서 체크' 로직은 CursorManager가 다~ 알아서 할 거예용!)
    // --- [!!! 삭제됨 끝 !!!] ---
}