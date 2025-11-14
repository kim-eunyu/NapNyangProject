using UnityEngine;
using TMPro; // TextMeshPro를 쓰기 위해 필요해용

public class QuestLogUI : MonoBehaviour
{
    // 1. 인스펙터에서 연결
    public GameObject mainQuestPage; // 메인 퀘스트 페이지 (GameObject)
    public GameObject subQuestPage;  // 서브 퀘스트 페이지 (GameObject)

    public TextMeshProUGUI mainQuestDescriptionText; // 메인 퀘스트 내용 텍스트
    // (나중에 여기에 서브 퀘스트 텍스트도 추가...)

    void Start()
    {
        // 2. 퀘스트 '내용' 이벤트를 구독 (텍스트 갱신용)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnMainQuestStepChanged.AddListener(UpdateMainQuestDescription);
        }
        
        // 3. 시작할 땐 퀘스트 창을 숨기고, 메인 페이지를 기본으로 설정
        ShowPage(mainQuestPage); // 메인 페이지를 기본으로
        gameObject.SetActive(false); // 퀘스트 창 전체를 숨김
    }

    // 4. QuestManager가 호출해줄 함수
    private void UpdateMainQuestDescription(string description)
    {
        if (mainQuestDescriptionText != null)
        {
            mainQuestDescriptionText.text = description;
        }
    }
    
    // 5. 페이지를 골라서 보여주는 내부 함수
    private void ShowPage(GameObject pageToShow)
    {
        if (mainQuestPage != null) mainQuestPage.SetActive(false);
        if (subQuestPage != null) subQuestPage.SetActive(false);

        if (pageToShow != null)
        {
            pageToShow.SetActive(true);
        }
    }

    // --- UI의 버튼들이 호출할 함수들 ---
    
    // 6. [중요] QuestIcon이 호출할 함수
    public void ToggleWindow()
    {
        // 현재 활성화 상태의 반대로 설정 (켜져있으면 끄고, 꺼져있으면 켬)
        bool isActive = !gameObject.activeSelf;
        gameObject.SetActive(isActive);

        // 퀘스트 창이 '켜질 때'
        if (isActive)
        {
            // 켜지는 순간, QuestManager로부터 최신 퀘스트 설명을 갱신해옴
            // (이벤트 발생 시점과 UI를 여는 시점이 다를 수 있으므로)
            if (QuestManager.Instance != null)
            {
                UpdateMainQuestDescription(QuestManager.Instance.GetCurrentMainQuestDescription());
            }
            
            // 켜질 땐 항상 메인 퀘스트 페이지부터 보여줌
            ShowPage(mainQuestPage);
        }
    }

    // 7. 메인 퀘스트 페이지 보기 (UI 버튼에 연결)
    public void ShowMainQuestPage()
    {
        ShowPage(mainQuestPage);
    }

    // 8. 서브 퀘스트 페이지 보기 (UI 버튼에 연결)
    public void ShowSubQuestPage()
    {
        ShowPage(subQuestPage);
    }
    
    // 9. 퀘스트 창 닫기 (UI의 'X' 버튼에 연결)
    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // 구독 해제
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnMainQuestStepChanged.RemoveListener(UpdateMainQuestDescription);
        }
    }
}