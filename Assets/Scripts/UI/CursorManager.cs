using UnityEngine;
using UnityEngine.EventSystems; // ⭐ 1. UI 이벤트를 쓰기 위해 이게 꼭 필요해용!
using System.Collections.Generic; // ⭐ 2. 리스트(List)를 쓰기 위해 이것도 필요해용!

// 으뉴님! UI도 감지하도록 수정했어용!
public class CursorManager : MonoBehaviour
{
    // --- 싱글톤 인스턴스 ---
    public static CursorManager Instance;

    // --- 텍스처 설정 ---
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D interactCursor;
    private Vector2 hotspot = Vector2.zero;

    // --- Awake() 함수 (싱글톤 처리) ---
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        SetDefaultCursor(); // (시작할 때 기본 커서로 설정)
    }

    // --- 4. Update() 로직 (UI 감지 기능 추가!) ---
    void Update()
    {
        // --- 1순위: UI 감지 (새로 추가된 부분!) ---

        // (1) 마우스 위치에 대한 포인터 이벤트 데이터를 만듭니다.
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;

        // (2) 마우스 위치에 있는 모든 UI 요소들을 담을 리스트를 만듭니다.
        List<RaycastResult> results = new List<RaycastResult>();

        // (3) UI 레이캐스트를 쏩니다! (EventSystem이 알아서 해줘용)
        EventSystem.current.RaycastAll(ped, results);

        // (4) 만약 UI가 하나라도 감지되었다면?
        if (results.Count > 0)
        {
            // 감지된 UI 중에서 "Interactable" 태그를 가진 녀석을 찾습니다.
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("Interactable"))
                {
                    // 찾았다! 상호작용 커서로 바꾸고
                    SetCursor(interactCursor);
                    // 이번 프레임의 커서 검사는 여기서 끝냅니다! (3D 검사 안 함)
                    return; 
                }
            }

            // 만약 UI가 감지됐지만 "Interactable" 태그는 없었다면?
            // (예: 그냥 빈 UI 패널 위)
            // 그냥 기본 커서로 설정하고 검사를 끝냅니다.
            SetDefaultCursor();
            return;
        }

        // --- 2순위: 3D 월드 감지 (기존 코드) ---
        // (UI가 아무것도 감지되지 않았을 때만 이 코드가 실행됩니다!)

        if (Camera.main == null)
        {
            return; // 안전장치
        }

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
                    SetDefaultCursor();
                    break;
            }
        }
        else
        {
            SetDefaultCursor();
        }
    }


    // --- 5. SetCursor 함수들 (기존과 동일) ---
    public void SetCursor(Texture2D cursorTexture)
    {
        if (cursorTexture != null)
        {
            Cursor.SetCursor(cursorTexture, hotspot, CursorMode.Auto);
        }
        else
        {
            SetDefaultCursor();
        }
    }

    public void SetDefaultCursor()
    {
        if (defaultCursor != null)
        {
            Cursor.SetCursor(defaultCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(null, hotspot, CursorMode.Auto);
        }
    }
}