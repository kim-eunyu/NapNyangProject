using UnityEngine;
using UnityEngine.EventSystems; 
using System.Collections.Generic; 
using UnityEngine.SceneManagement; // [!!! 1. 이게 '무조건' 필요해용 !!!]

// 으뉴님! UI 감지 + '사거리' 감지 + '씬 로딩' 감지 기능까지 넣었어용!
public class CursorManager : MonoBehaviour
{
    // --- 싱글톤 인스턴스 ---
    public static CursorManager Instance;

    // --- 텍스처 설정 ---
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D interactCursor;
    private Vector2 hotspot = Vector2.zero;

    // --- 플레이어 (씬이 바뀔 때마다 갱신될 변수) ---
    private Transform playerTransform;


    // --- Awake() 함수 (싱글톤 + 이벤트 구독) ---
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        
        SetDefaultCursor(); 
        
        // --- [!!! 2. 'Awake'에서 플레이어 찾기 '삭제' !!!] ---
        // (여기서 찾으면 '로비' 플레이어만 찾으니까, OnSceneLoaded에서 찾을 거예용)
        
        // --- [!!! 3. '씬 로드' 이벤트를 '구독' !!!] ---
        // "씬이 로드될 때마다 OnSceneLoaded 함수를 실행시켜줘!"
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    // --- [!!! 4. '구독 해제' (메모리 누수 방지용) !!!] ---
    private void OnDestroy()
    {
        // CursorManager가 (혹시라도) 파괴될 땐 구독을 해제해용.
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // --- [!!! 5. '새로운' 함수: 씬이 로드될 때마다 실행됨 !!!] ---
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬이 새로 로드됐으니, '새로운' 플레이어를 '다시' 찾아용!
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (playerTransform == null)
        {
            // '로비' 씬처럼 플레이어가 없는 씬일 수도 있으니, '에러' 대신 '로그'로 남겨용.
            Debug.Log($"CursorManager: '{scene.name}' 씬에 'Player' 태그를 가진 오브젝트가 없네용. (3D 커서 비활성화)");
        }
        else
        {
            Debug.Log($"CursorManager: '{scene.name}' 씬의 '새로운' 플레이어를 찾았어용!");
        }
        
        // (중요) 씬이 바뀌었으니 커서를 '기본'으로 리셋
        SetDefaultCursor();
    }
    
    // --- Update() 로직 (UI 감지 + 3D 사거리 감지!) ---
    void Update()
    {
        // --- 1순위: UI 감지 (으뉴님 코드) ---
        
        // [!!! 6. 'EventSystem.current'가 'null'일 때를 대비한 '안전장치' 추가 !!!]
        // (씬이 바뀌는 도중에 EventSystem이 잠깐 'null'이 될 수 있어용)
        if (EventSystem.current == null) 
        {
            // SetDefaultCursor(); // (이걸 넣으면 깜빡일 수 있으니, 그냥 둬도 돼용)
            return; // UI 시스템이 준비 안 됐으면 3D 검사도 하지 마!
        }

        PointerEventData ped = new PointerEventData(EventSystem.current);
        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);

        // (UI 감지 시 처리 - 으뉴님 코드와 100% 동일)
        if (results.Count > 0)
        {
            foreach (RaycastResult result in results)
            {
                if (result.gameObject.CompareTag("Interactable"))
                {
                    SetCursor(interactCursor);
                    return; 
                }
            }
            SetDefaultCursor();
            return; 
        }

        // --- 2순위: 3D 월드 감지 (UI가 감지되지 않았을 때) ---

        // (안전 장치 - 으뉴님 코드와 100% 동일)
        if (Camera.main == null) return;
        
        // [!!! 7. 이 'null 체크'가 '게임씬 002'에서도 정상 작동할 거예용 !!!]
        if (playerTransform == null) return; // '게임씬'의 플레이어를 못 찾았으면 3D 검사 안 함!

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            switch (hit.collider.tag)
            {
                // (이 'Monster' 로직은 으뉴님 코드와 100% 동일해용!)
                case "Monster":
                    Monster monster = hit.collider.GetComponent<Monster>();
                    if (monster != null)
                    {
                        float monsterAttackDistance = monster.attackDistance;
                        float distance = Vector3.Distance(playerTransform.position, monster.transform.position);

                        if (distance <= monsterAttackDistance)
                        {
                            SetCursor(attackCursor);
                        }
                        else
                        {
                            SetDefaultCursor();
                        }
                    }
                    else
                    {
                        SetDefaultCursor();
                    }
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