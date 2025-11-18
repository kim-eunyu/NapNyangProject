using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic; // Dictionary 쓰려면 필요!
using System.Text; // StringBuilder 쓰려면 필요!

[System.Serializable]
public class QuestStepEvent : UnityEvent<string> { }

[System.Serializable]
public class SubQuestUpdateEvent : UnityEvent<string> { }
// ---

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;
    
    // --- (메인 퀘스트 변수들은 동일) ---
    private MainQuestData mainQuest; 
    [SerializeField]
    private int currentStepIndex = 0;
    private bool isMainQuestActive = false;
    
    // --- (서브 퀘스트 변수들은 동일) ---
    private SubQuestData activeSubQuest; 
    private bool isSubQuestActive = false;
    private Dictionary<MonsterType, int> subQuestProgress;
    
    // --- (UI 이벤트들은 동일) ---
    public QuestStepEvent OnMainQuestStepChanged; 
    public UnityEvent OnNewQuestUpdate;           
    public SubQuestUpdateEvent OnSubQuestUpdated; 

    // --- (Awake, Start, 메인 퀘스트 함수들은 100% 동일) ---
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
    
    void Start() { } 

    public void StartMainQuest(MainQuestData questToStart)
    {
        if (isMainQuestActive) return;
        if (questToStart == null || questToStart.steps.Count == 0) return;

        mainQuest = questToStart;
        isMainQuestActive = true;
        currentStepIndex = 0;
        StartQuestStep(currentStepIndex); 
        OnNewQuestUpdate.Invoke(); 
    }
    
    public void AdvanceMainQuest()
    {
        if (!isMainQuestActive || mainQuest == null) return;

        if (currentStepIndex >= mainQuest.steps.Count - 1)
        {
            OnMainQuestStepChanged.Invoke("메인 퀘스트 완료!");
            isMainQuestActive = false;
            OnNewQuestUpdate.Invoke(); 
            return; 
        }

        currentStepIndex++; 
        StartQuestStep(currentStepIndex);
        OnNewQuestUpdate.Invoke(); 
    }

    private void StartQuestStep(int stepIndex)
    {
        if (mainQuest == null) return; 
        if (stepIndex < mainQuest.steps.Count)
        {
            OnMainQuestStepChanged.Invoke(mainQuest.steps[stepIndex].stepDescription);
        }
    }

    public int GetCurrentStepIndex()
    {
        return currentStepIndex;
    }
    
    // (이 함수는 '오타 수정판'과 동일)
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
    
    
    // --- [!!! 여기가 '최신' 버전이에용 (데이터 읽기) !!!] ---
    public string GetCurrentSubQuestDescription()
    {
        // 1. 퀘스트가 '진행 중'일 때 (기존과 동일)
        if (isSubQuestActive && activeSubQuest != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>{activeSubQuest.questName}</b>");
            
            foreach (var objective in activeSubQuest.objectives)
            {
                int currentAmount = 0;
                int requiredAmount = objective.requiredAmount;
                
                if (subQuestProgress != null && subQuestProgress.ContainsKey(objective.monsterType))
                {
                    currentAmount = subQuestProgress[objective.monsterType];
                }
                
                string line = $"- {objective.monsterType}: {currentAmount} / {requiredAmount}";
                
                if (currentAmount >= requiredAmount)
                {
                    sb.AppendLine($"<color=red><s>{line}</s></color>");
                }
                else
                {
                    sb.AppendLine(line);
                }
            }
            return sb.ToString();
        }
        // 2. [!!! 핵심 !!!] 퀘스트가 '완료'되었을 때
        else if (!isSubQuestActive && activeSubQuest != null)
        {
            // '데이터 파일'에서 으뉴님이 입력하신 '완료 문구'를 읽어옴!
            return activeSubQuest.completionMessage; 
        }

        // 3. 아무 퀘스트도 없을 때
        return "진행 중인 서브 퀘스트가 없습니다.";
    }
    // --- [!!! 수정된 부분 끝 !!!] ---
    
    
    // --- 서브 퀘스트 함수들 ---
    
    // (StartSubQuest 함수는 동일)
    public void StartSubQuest(SubQuestData questToStart)
    {
        if (isSubQuestActive) return; 
        
        activeSubQuest = questToStart;
        isSubQuestActive = true;
        
        subQuestProgress = new Dictionary<MonsterType, int>();
        foreach (var objective in activeSubQuest.objectives)
        {
            if (!subQuestProgress.ContainsKey(objective.monsterType))
            {
                subQuestProgress.Add(objective.monsterType, 0); 
            }
        }
        
        UpdateSubQuestUI(); 
        OnNewQuestUpdate.Invoke(); 
        Debug.Log($"서브 퀘스트 '{activeSubQuest.questName}' 시작!");
    }

    // (ReportMonsterKill 함수는 동일)
    public void ReportMonsterKill(MonsterType monsterType)
    {
        if (!isSubQuestActive) return; 
        
        if (subQuestProgress.ContainsKey(monsterType))
        {
            int required = GetRequiredAmount(monsterType);
            if (subQuestProgress[monsterType] < required)
            {
                subQuestProgress[monsterType]++; 
                UpdateSubQuestUI(); 
                CheckSubQuestCompletion();
            }
        }
    }

    // (UpdateSubQuestUI 함수는 동일 - 빨간 줄 그어주는 역할)
    private void UpdateSubQuestUI()
    {
        if (!isSubQuestActive) return;
        
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>{activeSubQuest.questName}</b>");
        
        foreach (var objective in activeSubQuest.objectives)
        {
            int currentAmount = subQuestProgress[objective.monsterType];
            int requiredAmount = objective.requiredAmount;
            string line = $"- {objective.monsterType}: {currentAmount} / {requiredAmount}";
                
            if (currentAmount >= requiredAmount)
            {
                sb.AppendLine($"<color=red><s>{line}</s></color>");
            }
            else
            {
                sb.AppendLine(line);
            }
        }
        OnSubQuestUpdated.Invoke(sb.ToString());
    }

    // --- [!!! 여기도 '최신' 버전이에용 (데이터 읽기) !!!] ---
    private void CheckSubQuestCompletion()
    {
        bool allCompleted = true;
        foreach (var objective in activeSubQuest.objectives)
        {
            if (subQuestProgress[objective.monsterType] < objective.requiredAmount)
            {
                allCompleted = false; 
                break;
            }
        }

        // [!!! 핵심 !!!] 모든 퀘스트가 완료되었을 때!
        if (allCompleted)
        {
            Debug.Log($"서브 퀘스트 '{activeSubQuest.questName}' 완료!");
            isSubQuestActive = false; // 퀘스트 상태를 '완료'로 변경
            
            // '데이터 파일'에서 으뉴님이 입력하신 '완료 문구'를 읽어옴!
            string completionMessage = activeSubQuest.completionMessage;
            
            // 1. UI에 '완료 메시지'를 쏴줌!
            OnSubQuestUpdated.Invoke(completionMessage);
            
            // 2. QuestIcon에 '반짝여라!' (띵동!) 신호를 쏴줌!
            OnNewQuestUpdate.Invoke(); 
        }
    }
    // --- [!!! 수정된 부분 끝 !!!] ---
    
    // (GetRequiredAmount 함수는 동일)
    private int GetRequiredAmount(MonsterType type)
    {
        foreach (var obj in activeSubQuest.objectives)
        {
            if (obj.monsterType == type) return obj.requiredAmount;
        }
        return 0;
    }
}