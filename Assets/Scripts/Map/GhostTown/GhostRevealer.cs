using UnityEngine;

public class GhostRevealer : MonoBehaviour
{
    [Header("👻 유령 설정")]
    [Tooltip("여기에 처음에 숨겨둘 유령 고양이 오브젝트를 끌어다 놓으세용!")]
    public GameObject ghostCat; // 유령 고양이 오브젝트를 담을 변수예용

    // 게임이 시작될 때 자동으로 실행되는 부분이에용
    void Start()
    {
        // 혹시 에디터에서 켜놨더라도, 게임 시작하면 일단 숨겨줄게용!
        if (ghostCat != null)
        {
            ghostCat.SetActive(false);
        }
    }

    // 무언가가 이 오브젝트의 콜라이더 영역에 들어오면 실행돼용!
    private void OnTriggerEnter(Collider other)
    {
        // 들어온 게 '플레이어'가 맞는지 태그로 확인해용!
        if (other.CompareTag("Player"))
        {
            // 유령 고양이가 할당되어 있다면 켜주세용!
            if (ghostCat != null)
            {
                ghostCat.SetActive(true);
                Debug.Log("유령 고양이 출현! 냐옹~ 🐱👻");
            }
        }
    }
}