// 파일 이름: SubQuestGiver.cs
using UnityEngine;
using UnityEngine.UI;

// MainQuestGiver.cs 랑 거의 똑같아용!
public class SubQuestGiver : MonoBehaviour
{
    // [!!! 유일한 차이 !!!]
    // MainQuestData 대신 SubQuestData 를 받아용
    public SubQuestData questToGive;

    // --- 대화창 변수들 (MainQuestGiver와 동일) ---
    [Header("Dialogue Settings")]
    public GameObject dialogueWindow; 
    public Button closeDialogueButton; 
    public float interactionDistance = 3.0f;
    private Transform playerTransform;
    
    private bool hasGivenQuest = false;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("SubQuestGiver: 'Player' 태그를 가진 플레이어를 찾을 수 없습니다!");
        }

        if (closeDialogueButton != null)
        {
            closeDialogueButton.onClick.AddListener(CloseDialogueWindow);
        }

        if (dialogueWindow != null)
        {
            dialogueWindow.SetActive(false);
        }
    }

    private void OnMouseDown()
    {
        if (playerTransform == null) return;
        
        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (distance <= interactionDistance)
        {
            // 1. 대화창을 켠다
            OpenDialogueWindow(); 
            // 2. 퀘스트를 주는 '시도'를 한다
            GiveQuest(); 
        }
    }

    private void GiveQuest()
    {
        if (hasGivenQuest)
        {
            Debug.Log("서브 퀘스트를 이미 받았습니다.");
            return; 
        }
        
        hasGivenQuest = true; 
        
        // [!!! 유일한 차이 !!!]
        // QuestManager의 'StartSubQuest' 함수를 호출해용
        QuestManager.Instance.StartSubQuest(questToGive);
        
        Debug.Log($"NPC({name}): 서브 퀘스트 '{questToGive.questName}'를 주었어용!");
    }

    // --- 대화창 여닫기 (MainQuestGiver와 동일) ---
    public void OpenDialogueWindow()
    {
        if (dialogueWindow != null) dialogueWindow.SetActive(true);
    }
    public void CloseDialogueWindow()
    {
        if (dialogueWindow != null) dialogueWindow.SetActive(false);
    }
}