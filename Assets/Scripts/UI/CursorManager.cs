using UnityEngine;
using UnityEngine.EventSystems; // ⭐ 1. UI 이벤트를 쓰기 위해 이게 꼭 필요해용!
using System.Collections.Generic; // ⭐ 2. 리스트(List)를 쓰기 위해 이것도 필요해용!

// 으뉴님! UI 감지 + '사거리' 감지 기능까지 넣었어용!
public class CursorManager : MonoBehaviour
{
    // --- 싱글톤 인스턴스 ---
    public static CursorManager Instance;

    // --- 텍스처 설정 ---
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D interactCursor;
    private Vector2 hotspot = Vector2.zero;

    // --- [!!! 새로 추가 !!!] ---
    // 3. 거리 계산을 위해 플레이어의 위치를 저장해 둘 변수
    private Transform playerTransform;
    // --- [!!! 새로 추가 끝 !!!] ---


    // --- Awake() 함수 (싱글톤 + 플레이어 찾기) ---
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
        
        // --- [!!! 새로 추가 !!!] ---
        // 4. 게임 시작 시 'Player' 태그로 플레이어를 찾아놔용.
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("CursorManager: 'Player' 태그를 가진 플레이어를 찾을 수 없습니다! 거리 계산이 불가능합니다.");
        }
        // --- [!!! 새로 추가 끝 !!!] ---
    }

    // --- Update() 로직 (UI 감지 + 3D 사거리 감지!) ---
    void Update()
    {
        // --- 1순위: UI 감지 (으뉴님 코드) ---

        // (1)~(3) UI 레이캐스트
        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        // (4) UI 감지 시 처리
        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("Interactable"))
                {
                    SetCursor(interactCursor);
                    return; // UI 감지했으니 3D 검사 안 함!
                }
            }
            SetDefaultCursor();
            return; // 'Interactable' 아닌 UI라도 3D 검사 안 함!
        }

        // --- 2순위: 3D 월드 감지 (UI가 감지되지 않았을 때) ---

        // (안전 장치)
        if (Camera.main == null) return;
        if (playerTransform == null) return; // 플레이어 없으면 3D 검사 안 함

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            switch (hit.collider.tag)
            {
                // --- [!!! 여기가 으뉴님이 원하신 핵심 로직 !!!] ---
                case "Monster":
                    // 1. 몬스터 태그가 감지되면, 'Monster.cs' 스크립트를 가져와봄
                    Monster monster = hit.collider.GetComponent<Monster>();
                    
                    // 2. 스크립트가 존재한다면?
                    if (monster != null)
                    {
                        // 3. 몬스터의 '공격 가능 거리' (attackDistance)를 가져옴
                        float monsterAttackDistance = monster.attackDistance;
                        // 4. 플레이어와 몬스터의 '현재 거리'를 실시간 계산!
                        float distance = Vector3.Distance(playerTransform.position, monster.transform.position);

                        // 5. [!!! 비교 !!!]
                        if (distance <= monsterAttackDistance)
                        {
                            // 사거리 안쪽! -> 칼 모양
                            SetCursor(attackCursor);
                        }
                        else
                        {
                            // 사거리 밖! -> 기본 모양 (으뉴님 요청)
                            SetDefaultCursor();
                        }
                    }
                    else
                    {
                        // (혹시 몬스터 태그는 있는데 스크립트가 없으면 그냥 기본 커서)
                        SetDefaultCursor();
                    }
                    break; // "Monster" 케이스 끝
                // --- [!!! 수정된 로직 끝 !!!] ---
                    
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


    // --- SetCursor 함수들 (으뉴님 코드) ---
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