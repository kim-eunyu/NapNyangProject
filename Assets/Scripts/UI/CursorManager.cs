using UnityEngine;

public class CursorManager : MonoBehaviour
{
    // 기본 커서 텍스처 (유니티 인스펙터에서 할당)
    [SerializeField] private Texture2D defaultCursor;

    // 공격(몬스터) 커서 텍스처
    [SerializeField] private Texture2D attackCursor;

    // 상호작용 커서 텍스처
    [SerializeField] private Texture2D interactCursor;

    // 커서의 클릭 지점 (hotspot). 보통 왼쪽 위가 (0,0)
    private Vector2 hotspot = Vector2.zero;

    // ✨ 게임이 시작되자마자 기본 커서를 설정해준다용!
    void Start()
    {
        SetDefaultCursor();
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            switch (hit.collider.tag)
            {
                case "Monster":
                    SetCursor(attackCursor);
                    break;
                case "Interactable":
                    SetCursor(interactCursor);
                    break;
                default:
                    SetDefaultCursor(); // 몬스터나 상호작용 오브젝트가 아니면 기본 커서로
                    break;
            }
        }
        else
        {
            SetDefaultCursor(); // 레이가 아무것에도 부딪히지 않았다면 기본 커서로
        }
    }

    private void SetCursor(Texture2D cursorTexture)
    {
        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
    }

    // ✨ 기본 커서를 설정하는 함수를 수정했어용!
    private void SetDefaultCursor()
    {
        // 이전에는 null을 넣었지만, 이제는 우리가 지정한 defaultCursor를 사용한다용
        Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
    }
}