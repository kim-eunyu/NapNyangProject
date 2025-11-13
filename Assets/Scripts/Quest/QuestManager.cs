// 파일 이름: QuestManager.cs
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class QuestStepEvent : UnityEvent<string> { }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    
    // [수정됨!] private으로 바꾸고, 
    // 나중에 퀘스트를 '받을' 때 이 변수에 할당할 거예용.
    private MainQuestData mainQuest; 
    
    [SerializeField]
    private int currentStepIndex = 0;
    
    // [새로 추가!] 퀘스트가 이미 진행 중인지 확인하는 깃발
    private bool isMainQuestActive = false;

    public QuestStepEvent OnMainQuestStepChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // [!!! 수정됨 !!!]
    // Start() 함수에서 퀘스트를 시작하는 코드를 *삭제*했어용.
    // 이제 QuestManager는 가만히 '대기'만 하고 있어용.
    void Start()
    {
        // (내용 비움)
    }

    // [!!! 핵심 함수 !!!]
    // NPC가 "이 퀘스트 시작해!"라고 부를 함수예용.
    public void StartMainQuest(MainQuestData questToStart)
    {
        // 퀘스트가 이미 진행 중이면 (메인 퀘스트는 하나니까) 또 시작하지 않아용.
        if (isMainQuestActive)
        {
            Debug.LogWarning("이미 메인 퀘스트가 진행 중입니다!");
            return;
        }

        // 퀘스트 데이터가 없으면 시작할 수 없어용.
        if (questToStart == null || questToStart.steps.Count == 0)
        {
            Debug.LogError("시작할 퀘스트 데이터가 없거나, 단계(Steps)가 비어있습니다!");
            return;
        }

        // 1. 관리자가 이 퀘스트를 '자신의 퀘스트'로 등록해용.
        this.mainQuest = questToStart;
        
        // 2. 퀘스트를 '진행 중' 상태로 바꿔용.
        this.isMainQuestActive = true;
        
        // 3. 0번 단계부터 시작해용.
        this.currentStepIndex = 0;
        StartQuestStep(currentStepIndex);
    }
    
    // (AdvanceMainQuest, GetCurrentStepIndex 함수는 그대로 두세용)

    private void StartQuestStep(int stepIndex)
    {
        // [안정성 코드 추가] 
        // 퀘스트가 아직 시작되지 않았으면(null이면) 아무것도 하지 않아용.
        if (mainQuest == null) return; 

        if (stepIndex < mainQuest.steps.Count)
        {
            string description = mainQuest.steps[stepIndex].stepDescription;
            OnMainQuestStepChanged.Invoke(description);
            Debug.Log($"[메인 퀘스트] 새 단계 시작 ({stepIndex}): {description}");
        }
    }
    
    public void AdvanceMainQuest()
    {
        // [안정성 코드 추가]
        if (!isMainQuestActive || mainQuest == null) return;

        if (currentStepIndex >= mainQuest.steps.Count - 1)
        {
            OnMainQuestStepChanged.Invoke("메인 퀘스트 완료!");
            Debug.Log("[메인 퀘스트] 모든 단계를 완료했습니다!");
            isMainQuestActive = false; // 퀘스트 완료!
            return; 
        }

        currentStepIndex++; 
        StartQuestStep(currentStepIndex); 
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
}