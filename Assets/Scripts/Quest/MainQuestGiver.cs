// 파일 이름: MainQuestGiver.cs
using UnityEngine;

// [!!! 수정됨 !!!] 클래스 이름을 MainQuestGiver로 변경했어용.
public class MainQuestGiver : MonoBehaviour
{
    // 1. 인스펙터에서 이 NPC가 줄 퀘스트(.asset 파일)를 연결해용.
    public MainQuestData questToGive;

    // 2. 퀘스트를 이미 줬는지 확인 (중복 방지)
    private bool hasGivenQuest = false;

    // 3. 상호작용이 가능한 최대 거리
    public float interactionDistance = 3.0f; // 3미터

    // 4. 플레이어 오브젝트 (거리 계산용)
    private Transform playerTransform;

    private void Start()
    {
        // 게임 시작 시 "Player" 태그를 가진 오브젝트를 찾아놔용.
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            // [!!! 수정됨 !!!] 에러 메시지도 스크립트 이름을 반영했어용.
            Debug.LogError("MainQuestGiver: 'Player' 태그를 가진 플레이어를 찾을 수 없습니다!");
        }
    }

    // 이 오브젝트에 붙은 'Collider'가 마우스 왼쪽 버튼으로 클릭되었을 때 호출돼용.
    private void OnMouseDown()
    {
        // 1. 퀘스트를 이미 줬으면 아무것도 안 함
        if (hasGivenQuest)
        {
            Debug.Log("이미 퀘스트를 받았습니다.");
            return;
        }

        // 2. 플레이어를 못 찾았으면 아무것도 안 함
        if (playerTransform == null)
        {
            Debug.LogError("플레이어가 없어서 퀘스트를 줄 수 없습니다.");
            return;
        }

        // 3. 플레이어와 NPC 사이의 거리를 계산해용.
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        // 4. 거리가 interactionDistance(3.0f) 이내일 때만 퀘스트를 줘용.
        if (distance <= interactionDistance)
        {
            GiveQuest();
        }
        else
        {
            Debug.Log("너무 멀리 있어서 NPC와 대화할 수 없습니다.");
        }
    }

    private void GiveQuest()
    {
        // 1. 퀘스트를 줬다고 표시 (다시 못 받게)
        hasGivenQuest = true;

        // 2. (중요!) QuestManager에게 퀘스트를 시작하라고 명령!
        QuestManager.Instance.StartMainQuest(questToGive);
        
        Debug.Log($"NPC({name}): 퀘스트 '{questToGive.questName}'를 주었어용!");
    }
}