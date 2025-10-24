using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false; 

    // 연결 변수들 (그대로)
    [SerializeField] private GameObject batAdoptWindow; // 이 상자가 켤 팝업창
    [SerializeField] private GameObject batPet;       // 이 상자가 켤 '플레이어 펫'
    [SerializeField] private GameObject chestBat;     // 이 상자가 껐다 켰다 할 '상자 안 박쥐'

    // ... (Awake 함수는 100% 동일!) ...
    void Awake()
    {
        animator = GetComponent<Animator>();
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

    // --- ✨ 여기가 바뀌었어용! 'OnMouseDown' 함수! ---
    void OnMouseDown()
    {
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

            // --- ✨ 여기가 추가됐어용! ---
            // 상자를 열 때마다, 상자 안 박쥐를 '초기화' (무조건 다시 켜기!)
            if (chestBat != null)
            {
                chestBat.SetActive(true); 
            }
            // --- ✨ 여기까지! ---

            // (UI 켜는 로직 - 그대로)
            GameObject[] allPopups = GameObject.FindGameObjectsWithTag("UIPopup");
            foreach (GameObject popup in allPopups)
            {
                popup.SetActive(false);
            }
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

            // (입양창 끄기 - 그대로)
            if (batAdoptWindow != null)
            {
                batAdoptWindow.SetActive(false);
            }
        }
    }

    // ... (AdoptBat 함수는 100% 동일!) ...
    public void AdoptBat()
    {
        Debug.Log(">>> 박쥐를 입양했습니다! 🦇");

        // (1) 모든 'BatPet' 태그 펫 끄기
        GameObject[] allBatPets = GameObject.FindGameObjectsWithTag("BatPet");
        foreach (GameObject pet in allBatPets)
        {
            pet.SetActive(false);
        }

        // (2) '내' 펫 켜기
        if (batPet != null)
        {
            batPet.SetActive(true);
        }

        // (3) 상자 안의 '원조 박쥐' 끄기
        if (chestBat != null)
        {
            chestBat.SetActive(false);
        }

        // (4) 입양 창 닫기
        if (batAdoptWindow != null)
        {
            batAdoptWindow.SetActive(false);
        }
    }
}