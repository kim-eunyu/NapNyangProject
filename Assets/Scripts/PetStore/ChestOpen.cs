using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false; 

    // ✨ 1. 여기에 박쥐 입양 UI 패널을 연결할 변수를 추가!
    [SerializeField] private GameObject batAdoptWindow;

    void Awake()
    {
        animator = GetComponent<Animator>();

        // (탐지기 1번 - 그대로!)
        if (animator == null)
        {
            Debug.LogError("======= [치명적인 에러!] =======");
            Debug.LogError(gameObject.name + " 오브젝트에서 Animator 컴포넌트를 못 찾았다용! 😫");
            Debug.LogError("================================");
        }
        else
        {
            Debug.Log(gameObject.name + "에서 Animator 찾기 성공! (Awake)");
        }
    }

    void OnMouseDown()
    {
        // (탐지기 2, 3번 - 그대로!)
        Debug.Log("OnMouseDown CLICKED! --- 클릭 감지 성공! ---");
        if (animator == null)
        {
            Debug.LogError("클릭은 됐지만 animator가 비어있다용! (null)");
            return; 
        }

        // --- 상자 '열 때' ---
        if (isOpened == false)
        {
            isOpened = true; 
            animator.SetTrigger("Open"); 
            Debug.Log(">>> 상자 열기 신호 (Open) 보냄!");

            // --- ✨ 2. 여기에 UI 켜는 로직을 추가! ---
            // 먼저, 태그가 "UIPopup"인 창을 모두 끈다.
            GameObject[] allPopups = GameObject.FindGameObjectsWithTag("UIPopup");
            foreach (GameObject popup in allPopups)
            {
                popup.SetActive(false);
            }

            // 그 다음, 박쥐 입양창을 켠다.
            if (batAdoptWindow != null)
            {
                batAdoptWindow.SetActive(true);
            }
        }
        // --- 상자 '닫을 때' ---
        else 
        {
            isOpened = false; 
            animator.SetTrigger("Close"); 
            Debug.Log(">>> 상자 닫기 신호 (Close) 보냄!");

            // --- ✨ 3. (안전장치) 상자를 닫을 때 입양창도 끈다! ---
            if (batAdoptWindow != null)
            {
                batAdoptWindow.SetActive(false);
            }
        }
    }

    // --- ✨ 4. 'O' 버튼이 눌렀을 때 실행할 함수 추가! (public 필수!) ---
    public void AdoptBat()
    {
        // 여기에 입양했을 때 실행할 코드를 넣으면 돼용
        Debug.Log(">>> 박쥐를 입양했습니다! 🦇");

        // 그리고 창을 닫는다!
        if (batAdoptWindow != null)
        {
            batAdoptWindow.SetActive(false);
        }
    }
}