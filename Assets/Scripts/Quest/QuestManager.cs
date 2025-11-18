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
    
    // --- 메인 퀘스트 변수들 ---
    private MainQuestData mainQuest; 
    [SerializeField]
    private int currentStepIndex = 0;
    private bool isMainQuestActive = false;
    
    // --- 서브 퀘스트 변수들 ---
    private SubQuestData activeSubQuest; 
    private bool isSubQuestActive = false;
    private Dictionary<MonsterType, int> subQuestProgress;
    // ---

    // --- UI 이벤트들 ---
    public QuestStepEvent OnMainQuestStepChanged; 
    public UnityEvent OnNewQuestUpdate;           
    public SubQuestUpdateEvent OnSubQuestUpdated; 
    // ---

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

    // --- 메인 퀘스트 함수들 ---
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
    
    // --- [!!! 이 함수가 새로 추가되었어용 !!!] ---
    public string GetCurrentSubQuestDescription()
    {
        if (isSubQuestActive && activeSubQuest != null)
        {
            // UpdateSubQuestUI() 함수와 동일한 로직으로 텍스트를 만들어 반환해용.
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"<b>{activeSubQuest.questName}</b>");
            
            foreach (var objective in activeSubQuest.objectives)
            {
                // (안정성 체크) 딕셔너리가 초기화되었는지 확인
                if (subQuestProgress != null && subQuestProgress.ContainsKey(objective.monsterType))
                {
                    int currentAmount = subQuestProgress[objective.monsterType];
                    sb.AppendLine($"- {objective.monsterType}: {currentAmount} / {objective.requiredAmount}");
                }
                else
                {
                     // (방금 퀘스트를 받아서 딕셔너리가 아직 없을 경우 대비)
                     sb.AppendLine($"- {objective.monsterType}: 0 / {objective.requiredAmount}");
                }
            }
            return sb.ToString();
        }
        else if (!isSubQuestActive && activeSubQuest != null)
        {
            return $"<b>{activeSubQuest.questName} (완료!)</b>";
        }

        return "진행 중인 서브 퀘스트가 없습니다.";
    }
    // --- [!!! 추가된 부분 끝 !!!] ---
    
    
    // --- 서브 퀘스트 함수들 ---
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

    private void UpdateSubQuestUI()
    {
        if (!isSubQuestActive) return;
        
        // UI에 보낼 텍스트 만들기 (GetCurrentSubQuestDescription과 동일 로직)
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>{activeSubQuest.questName}</b>");
        foreach (var objective in activeSubQuest.objectives)
        {
            int currentAmount = subQuestProgress[objective.monsterType];
            sb.AppendLine($"- {objective.monsterType}: {currentAmount} / {objective.requiredAmount}");
        }
        
        OnSubQuestUpdated.Invoke(sb.ToString());
    }

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

        if (allCompleted)
        {
            Debug.Log($"서브 퀘스트 '{activeSubQuest.questName}' 완료!");
            isSubQuestActive = false;
            OnSubQuestUpdated.Invoke($"<b>{activeSubQuest.questName} (완료!)</b>");
            OnNewQuestUpdate.Invoke(); 
        }
    }
    
    private int GetRequiredAmount(MonsterType type)
    {
        foreach (var obj in activeSubQuest.objectives)
        {
            if (obj.monsterType == type) return obj.requiredAmount;
        }
        return 0;
    }
}