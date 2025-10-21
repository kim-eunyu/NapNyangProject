using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false; 

    void Awake()
    {
        animator = GetComponent<Animator>();

        // ★★★ 탐지기 1번 ★★★
        // Awake()가 실행될 때, animator를 제대로 찾았는지 확인!
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
        // ★★★ 탐지기 2번 ★★★
        // OnMouseDown 함수가 '일단 실행되는지' 확인!
        Debug.Log("OnMouseDown CLICKED! --- 클릭 감지 성공! ---");

        // ★★★ 탐지기 3번 ★★★
        // 클릭은 됐는데, animator 변수가 비어있진 않은지(null인지) 확인!
        if (animator == null)
        {
            Debug.LogError("클릭은 됐지만 animator가 비어있다용! (null)");
            return; // 함수를 즉시 종료
        }

        // --- (여긴 원래 코드) ---
        if (isOpened == false)
        {
            isOpened = true; 
            animator.SetTrigger("Open"); 
            Debug.Log(">>> 상자 열기 신호 (Open) 보냄!");
        }
        else 
        {
            isOpened = false; 
            animator.SetTrigger("Close"); 
            Debug.Log(">>> 상자 닫기 신호 (Close) 보냄!");
        }
    }
}