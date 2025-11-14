using UnityEngine;
using UnityEngine.UI; // [!!! 새로 추가 !!!] Button을 쓰려면 이게 필요해용.

public class MainQuestGiver : MonoBehaviour
{
    // --- 기존 변수들 ---
    public MainQuestData questToGive;
    private bool hasGivenQuest = false;
    public float interactionDistance = 3.0f;
    private Transform playerTransform;

    // --- [!!! 새로 추가된 변수들 !!!] ---
    [Header("Dialogue Settings")]
    public GameObject dialogueWindow; // 으뉴님이 만든 '대화창' Panel 오브젝트
    public Button closeDialogueButton; // 대화창의 'X' 버튼

    // --- Start 함수 ---
    private void Start()
    {
        // 플레이어 찾기 (기존과 동일)
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("MainQuestGiver: 'Player' 태그를 가진 플레이어를 찾을 수 없습니다!");
        }

        // --- [!!! 새로 추가된 로직 !!!] ---
        
        // 1. 'X' 버튼이 눌리면 CloseDialogueWindow 함수를 실행하도록 '구독(연결)'
        if (closeDialogueButton != null)
        {
            closeDialogueButton.onClick.AddListener(CloseDialogueWindow);
        }
        else
        {
            Debug.LogWarning($"MainQuestGiver ({name}): X 버튼이 연결되지 않았습니다!");
        }

        // 2. 게임 시작 시, 대화창은 무조건 꺼둠
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(false);
        }
    }

    // --- OnMouseDown 함수 (수정됨) ---
    private void OnMouseDown()
    {
        if (playerTransform == null) return;
        
        // 1. 거리 체크 (기존과 동일)
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        // 2. 거리가 가까우면?
        if (distance <= interactionDistance)
        {
            // [!!! 핵심 1 !!!]
            // 대화창을 켠다! (으뉴님 요청: "누를 때마다")
            OpenDialogueWindow(); 

            // [!!! 핵심 2 !!!]
            // 퀘스트를 주는 '시도'를 한다.
            GiveQuest(); 
        }
        else
        {
            Debug.Log("너무 멀리 있어서 NPC와 대화할 수 없습니다.");
        }
    }

    // --- GiveQuest 함수 (수정됨) ---
    // (이름은 그대로 두되, 중복 방지 로직이 핵심)
    private void GiveQuest()
    {
        // 1. 퀘스트를 '이미 줬다면'?
        if (hasGivenQuest)
        {
            Debug.Log("퀘스트를 이미 받았습니다. (대화창만 엽니다)");
            return; // 함수를 즉시 종료 (퀘스트를 또 주지 않음)
        }
        
        // 2. 퀘스트를 '처음 주는 거라면'?
        hasGivenQuest = true; // 줬다고 표시
        QuestManager.Instance.StartMainQuest(questToGive); // 매니저에게 퀘스트 시작 명령
        Debug.Log($"NPC({name}): 퀘스트 '{questToGive.questName}'를 주었어용!");
    }

    // --- [!!! 새로 추가된 함수들 !!!] ---

    // 대화창을 여는 함수
    public void OpenDialogueWindow()
    {
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(true);
        }
    }

    // 'X' 버튼이 호출할, 대화창을 닫는 함수
    public void CloseDialogueWindow()
    {
        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(false);
        }
    }
}