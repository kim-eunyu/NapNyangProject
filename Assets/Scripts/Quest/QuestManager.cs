using UnityEngine;
using UnityEngine.Events;

// (이 부분은 그대로)
[System.Serializable]
public class QuestStepEvent : UnityEvent<string> { }
// ---

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    
    private MainQuestData mainQuest; 
    [SerializeField]
    private int currentStepIndex = 0;
    private bool isMainQuestActive = false;

    // --- 1. UI 연동용 이벤트 ---
    
    // [기존 이벤트] 퀘스트 '내용(string)'을 전달하는 이벤트
    public QuestStepEvent OnMainQuestStepChanged; 

    // [!!! 새로 추가 !!!]
    // 퀘스트가 갱신됐다는 '단순 신호'만 보내는 이벤트예용.
    // (내용물 없이 "띵동!" 하고 벨만 누르는 거)
    public UnityEvent OnNewQuestUpdate;
    
    // --- (Awake, Start 함수는 기존과 동일) ---
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
    void Start() { } // 비워둠

    // --- [!!! 수정됨 !!!] ---
    public void StartMainQuest(MainQuestData questToStart)
    {
        if (isMainQuestActive) return;
        if (questToStart == null || questToStart.steps.Count == 0) return;

        this.mainQuest = questToStart;
        this.isMainQuestActive = true;
        this.currentStepIndex = 0;
        
        StartQuestStep(currentStepIndex); // 0번 단계 시작
        
        // [!!! 새로 추가 !!!]
        // "새 퀘스트 받았어!" 라고 UI에 띵동! 신호 보내기
        OnNewQuestUpdate.Invoke();
    }
    
    private void StartQuestStep(int stepIndex)
    {
        if (mainQuest == null) return; 

        if (stepIndex < mainQuest.steps.Count)
        {
            string description = mainQuest.steps[stepIndex].stepDescription;
            // 퀘스트 '내용' 전달 (이건 QuestLogUI가 쓸 거예용)
            OnMainQuestStepChanged.Invoke(description);
        }
    }

    // --- [!!! 수정됨 !!!] ---
    public void AdvanceMainQuest()
    {
        if (!isMainQuestActive || mainQuest == null) return;

        if (currentStepIndex >= mainQuest.steps.Count - 1)
        {
            OnMainQuestStepChanged.Invoke("메인 퀘스트 완료!");
            isMainQuestActive = false;
            
            // [!!! 새로 추가 !!!]
            // "퀘스트 완료됐어!" 라고 UI에 띵동! 신호 보내기
            OnNewQuestUpdate.Invoke();
            return; 
        }

        currentStepIndex++; 
        StartQuestStep(currentStepIndex);
        
        // [!!! 새로 추가 !!!]
        // "다음 단계로 넘어갔어!" 라고 UI에 띵동! 신호 보내기
        OnNewQuestUpdate.Invoke();
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
    
    // (선택) 퀘스트 로그 UI가 현재 퀘스트 설명을 다시 요청할 때 쓸 함수
    public string GetCurrentMainQuestDescription()
    {
        if (isMainQuestActive && mainQuest != null && currentStepIndex < mainQuest.steps.Count)
        {
            return mainQuest.steps[currentStepIndex].stepDescription;
        }
        else if (!isMainQuestActive && mainQuest != null)
        {
             return "메인 퀘스트 완료!";
        }
        return "진행 중인 메인 퀘스트가 없습니다.";
    }
}