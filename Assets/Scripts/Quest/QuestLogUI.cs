using UnityEngine;
using TMPro; 

public class QuestLogUI : MonoBehaviour
{
    // --- 변수들 (기존과 동일) ---
    public GameObject mainQuestPage; 
    public GameObject subQuestPage;  
    public TextMeshProUGUI mainQuestDescriptionText; 
    
    [Header("Sub Quest UI")]
    public TextMeshProUGUI subQuestDescriptionText; 

    void Start()
    {
        if (QuestManager.Instance != null)
        {
            // 메인/서브 퀘스트 이벤트 둘 다 구독
            QuestManager.Instance.OnMainQuestStepChanged.AddListener(UpdateMainQuestDescription);
            QuestManager.Instance.OnSubQuestUpdated.AddListener(UpdateSubQuestDescription);
        }
        
        ShowPage(mainQuestPage); 
        gameObject.SetActive(false); 
    }

    // --- 메인 퀘스트 UI 함수 ---
    private void UpdateMainQuestDescription(string description)
    {
        if (mainQuestDescriptionText != null)
        {
            mainQuestDescriptionText.text = description;
        }
    }
    
    // --- 서브 퀘스트 UI 함수 ---
    private void UpdateSubQuestDescription(string description)
    {
        if (subQuestDescriptionText != null)
        {
            subQuestDescriptionText.text = description;
        }
    }
    
    private void ShowPage(GameObject pageToShow)
    {
        if (mainQuestPage != null) mainQuestPage.SetActive(false);
        if (subQuestPage != null) subQuestPage.SetActive(false);
        if (pageToShow != null) pageToShow.SetActive(true);
    }

    // --- [!!! 이 함수가 수정되었어용 (버그 수정) !!!] ---
    public void ToggleWindow()
    {
        bool isActive = !gameObject.activeSelf;
        gameObject.SetActive(isActive);

        if (isActive)
        {
            // 켜질 때 메인/서브 퀘스트 내용을 둘 다 새로고침!
            if (QuestManager.Instance != null)
            {
                // 1. 메인 퀘스트 새로고침
                UpdateMainQuestDescription(QuestManager.Instance.GetCurrentMainQuestDescription());
                
                // 2. [!!! 여기가 수정된 부분 !!!]
                //    서브 퀘스트도 새로고침
                UpdateSubQuestDescription(QuestManager.Instance.GetCurrentSubQuestDescription());
            }
            // 켜질 땐 항상 메인 페이지부터 보여줌
            ShowPage(mainQuestPage);
        }
    }
    // --- [!!! 수정된 부분 끝 !!!] ---

    public void ShowMainQuestPage()
    {
        ShowPage(mainQuestPage);
    }
    public void ShowSubQuestPage()
    {
        ShowPage(subQuestPage);
    }
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnMainQuestStepChanged.RemoveListener(UpdateMainQuestDescription);
            QuestManager.Instance.OnSubQuestUpdated.RemoveListener(UpdateSubQuestDescription);
        }
    }
}