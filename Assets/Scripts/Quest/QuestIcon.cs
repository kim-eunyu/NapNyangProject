using UnityEngine;
using UnityEngine.UI; // Button을 쓰기 위해 필요해용

public class QuestIcon : MonoBehaviour
{
    // 1. 인스펙터에서 연결
    public GameObject redDot; // 퀘스트 알림용 빨간 점
    public Animator iconAnimator; // 커졌다 작아졌다 할 애니메이터
    public QuestLogUI questLogUI; // 클릭하면 열릴 퀘스트 창

    // 2. 퀘스트 상태
    private bool hasNewUpdate = false;

    void Start()
    {
        // 3. QuestManager의 '띵동!' 신호를 구독
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnNewQuestUpdate.AddListener(HandleNewUpdate);
        }
        
        // 4. 시작할 땐 다 숨기기
        if (redDot != null) redDot.SetActive(false);
        if (iconAnimator != null) iconAnimator.SetBool("HasUpdate", false);
    }

    // [!!! 핵심 !!!]
    // 5. '띵동!' 신호가 오면 이 함수가 실행돼용
    private void HandleNewUpdate()
    {
        hasNewUpdate = true;
        if (redDot != null) redDot.SetActive(true);
        
        // "HasUpdate"라는 파라미터를 true로 바꿔서 애니메이션을 실행시켜용
        if (iconAnimator != null) iconAnimator.SetBool("HasUpdate", true);
    }

    // [!!! 중요 !!!]
    // 6. 이 스크립트가 붙은 UI의 'Button' 컴포넌트가 OnClick()으로 호출할 함수!
    public void OnIconClicked()
    {
        // 7. 아이콘을 클릭하면 알림 상태를 해제
        if (hasNewUpdate)
        {
            hasNewUpdate = false;
            if (redDot != null) redDot.SetActive(false);
            
            // "HasUpdate" 파라미터를 false로 바꿔서 애니메이션을 멈춰용
            if (iconAnimator != null) iconAnimator.SetBool("HasUpdate", false);
        }

        // 8. 퀘스트 창을 열어달라고 요청
        if (questLogUI != null)
        {
            questLogUI.ToggleWindow(); // 퀘스트 창을 켜거나 끄게 해용
        }
    }

    private void OnDestroy()
    {
        // 구독 해제 (씬 이동 시 메모리 누수 방지)
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.OnNewQuestUpdate.RemoveListener(HandleNewUpdate);
        }
    }
}